using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiFileTests
    {
        #region Nested classes

        private sealed class PayloadCustomMetaEvent : MetaEvent
        {
            public const byte StatusByte = 0x5B;

            public byte[]? Payload { get; set; }

            public string? Description { get; set; }

            protected override MidiEvent CloneEvent() =>
                new PayloadCustomMetaEvent { Payload = Payload, Description = Description };

            protected override int GetContentSize(WritingSettings settings) => Payload?.Length ?? 0;

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
            {
                Payload = reader.ReadBytes(size);
                Description = new string('a', size);
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                if (Payload != null)
                    writer.WriteBytes(Payload);
            }

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                int size,
                CancellationToken cancellationToken = default)
            {
                Payload = await reader.ReadBytesAsync(size, cancellationToken).ConfigureAwait(false);
                Description = new string('a', size);
            }

            protected override async Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken = default)
            {
                if (Payload != null)
                    await writer.WriteBytesAsync(Payload, cancellationToken).ConfigureAwait(false);
            }
        }

        private sealed class PayloadCustomChunk : MidiChunk
        {
            public const string Id = "Pyld";

            public PayloadCustomChunk()
                : base(Id)
            {
            }

            public int[]? Values { get; private set; }

            public List<string>? Strings { get; private set; }

            public PayloadCustomChunk(int size)
                : this()
            {
                Values = new int[size];
            }

            public override MidiChunk Clone() => new PayloadCustomChunk { Values = Values };

            protected override uint GetContentSize(WritingSettings settings) => (uint)((Values?.Length ?? 0) * sizeof(int));

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                var count = (int)(size / sizeof(int));
                Values = new int[count];
                Strings = new List<string>();
                for (var i = 0; i < count; i++)
                {
                    Values[i] = (int)reader.ReadDword();
                    Strings.Add(new string('b', 10));
                }
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                foreach (var value in Values ?? Array.Empty<int>())
                {
                    writer.WriteDword((uint)value);
                }
            }

            protected override async Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                uint size,
                CancellationToken cancellationToken)
            {
                var count = (int)(size / sizeof(int));
                Values = new int[count];
                Strings = new List<string>();

                for (var i = 0; i < count; i++)
                {
                    Values[i] = (int)await reader.ReadDwordAsync(cancellationToken).ConfigureAwait(false);
                    Strings.Add(new string('b', 10));
                }
            }

            protected override async Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken)
            {
                foreach (var value in Values ?? Array.Empty<int>())
                {
                    await writer.WriteDwordAsync((uint)value, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Test methods

        internal static IEnumerable<MidiEvent> GetEventsForMaxMemorySize()
        {
            yield return new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100);
            yield return new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0);
            yield return new NoteAftertouchEvent((SevenBitNumber)60, (SevenBitNumber)10);
            yield return new ControlChangeEvent((SevenBitNumber)1, (SevenBitNumber)10);
            yield return new ProgramChangeEvent((SevenBitNumber)10);
            yield return new ChannelAftertouchEvent((SevenBitNumber)10);
            yield return new PitchBendEvent(1000);
            yield return new SequenceNumberEvent(1);
            yield return new TextEvent("text");
            yield return new CopyrightNoticeEvent("copyright");
            yield return new SequenceTrackNameEvent("name");
            yield return new InstrumentNameEvent("instrument");
            yield return new LyricEvent("lyric");
            yield return new MarkerEvent("marker");
            yield return new CuePointEvent("cue");
            yield return new ProgramNameEvent("program");
            yield return new DeviceNameEvent("device");
            yield return new ChannelPrefixEvent(1);
            yield return new PortPrefixEvent(1);
            yield return new SetTempoEvent(500000);
            yield return new SmpteOffsetEvent(SmpteFormat.TwentyFour, 1, 2, 3, 4, 5);
            yield return new TimeSignatureEvent(3, 4);
            yield return new KeySignatureEvent(1, 0);
            yield return new SequencerSpecificEvent(new byte[] { 1, 2, 3 });
            yield return new NormalSysExEvent(new byte[] { 1, 2, 3, 0xF7 });
            yield return new EscapeSysExEvent(new byte[] { 1, 2, 3 });
        }

        [TestCaseSource(nameof(GetEventsForMaxMemorySize))]
        public void Read_MaxMemorySize_EventType(MidiEvent midiEvent)
        {
            const int count = 100;

            var bytes = GetFileBytes(new TrackChunk(Enumerable.Range(0, count).Select(_ => midiEvent.Clone())));

            // Every event takes at least object header, delta time and slot in the collection
            var minimalLimit = GetMinimalMemoryLimit(bytes);
            ClassicAssert.GreaterOrEqual(minimalLimit, count * 32L, $"Estimated size is too small for {midiEvent.GetType().Name}.");
            ClassicAssert.Less(minimalLimit, count * 1024L, $"Estimated size is too big for {midiEvent.GetType().Name}.");

            AssertThrowsTooLarge(bytes, minimalLimit - 1);
        }

        [TestCase(10000)]
        [TestCase(100000)]
        public void Read_MaxMemorySize_TextEvent_PayloadIsCounted(int length)
        {
            var bytes = GetFileBytes(new TrackChunk(new TextEvent(new string('a', length))));

            // Strings are stored as UTF-16 in memory
            ClassicAssert.GreaterOrEqual(GetMinimalMemoryLimit(bytes), length * 2L);
        }

        [TestCase(10000)]
        [TestCase(100000)]
        public void Read_MaxMemorySize_DataEvents_PayloadIsCounted(int length)
        {
            var data = new byte[length];

            foreach (var midiEvent in new MidiEvent[]
            {
                new SequencerSpecificEvent(data),
                new NormalSysExEvent(data),
                new EscapeSysExEvent(data)
            })
            {
                var bytes = GetFileBytes(new TrackChunk(midiEvent));
                ClassicAssert.GreaterOrEqual(
                    GetMinimalMemoryLimit(bytes),
                    length,
                    $"Payload of {midiEvent.GetType().Name} is not counted.");
            }
        }

        [Test]
        public void Read_MaxMemorySize_UnknownMetaEvent_PayloadIsCounted()
        {
            var bytes = GetFileBytes(
                new TrackChunk(new PayloadCustomMetaEvent { Payload = new byte[10000] }),
                new WritingSettings { CustomMetaEventTypes = new EventTypesCollection { { typeof(PayloadCustomMetaEvent), PayloadCustomMetaEvent.StatusByte } } });

            // No custom types registered on reading, so the event is read as UnknownMetaEvent
            var minimalLimit = GetMinimalMemoryLimit(bytes);
            ClassicAssert.GreaterOrEqual(minimalLimit, 10000);
            AssertThrowsTooLarge(bytes, minimalLimit - 1);
        }

        [Test]
        public void Read_MaxMemorySize_CustomMetaEvent_AllFieldsAreCounted()
        {
            var customTypes = new EventTypesCollection { { typeof(PayloadCustomMetaEvent), PayloadCustomMetaEvent.StatusByte } };
            var bytes = GetFileBytes(
                new TrackChunk(new PayloadCustomMetaEvent { Payload = new byte[10000] }),
                new WritingSettings { CustomMetaEventTypes = customTypes });

            // 10000 bytes of the array and 10000 characters (20000 bytes) of the string
            var minimalLimit = GetMinimalMemoryLimit(bytes, new ReadingSettings { CustomMetaEventTypes = customTypes });
            ClassicAssert.GreaterOrEqual(minimalLimit, 30000);
            AssertThrowsTooLarge(bytes, 29999, new ReadingSettings { CustomMetaEventTypes = customTypes });
        }

        [Test]
        public void Read_MaxMemorySize_UnknownChunk_PayloadIsCounted()
        {
            var bytes = GetFileBytes(new UnknownChunk("Abcd") { Data = new byte[100000] });

            var minimalLimit = GetMinimalMemoryLimit(bytes);
            ClassicAssert.GreaterOrEqual(minimalLimit, 100000);
            AssertThrowsTooLarge(bytes, 99999);
        }

        [Test]
        public void Read_MaxMemorySize_CustomChunk_AllFieldsAreCounted()
        {
            var chunkTypes = new ChunkTypesCollection { { typeof(PayloadCustomChunk), PayloadCustomChunk.Id } };
            var bytes = GetFileBytes(new PayloadCustomChunk(1000));

            // 1000 ints (4000 bytes) and 1000 strings of 10 characters
            var readingSettings = new ReadingSettings { CustomChunkTypes = chunkTypes };
            var minimalLimit = GetMinimalMemoryLimit(bytes, readingSettings);
            ClassicAssert.GreaterOrEqual(minimalLimit, 4000 + 1000 * 20);
            AssertThrowsTooLarge(bytes, 4000, readingSettings);

            var file = MidiFile.Read(new MemoryStream(bytes), new ReadingSettings { CustomChunkTypes = chunkTypes, MaxMemorySize = minimalLimit });
            ClassicAssert.AreEqual(1000, file.Chunks.OfType<PayloadCustomChunk>().Single().Values!.Length, "Custom chunk payload length is invalid.");

            using (var stream = new MemoryStream(bytes))
            {
                file = MidiFile.ReadAsync(
                    stream,
                    new ReadingSettings { CustomChunkTypes = chunkTypes, MaxMemorySize = minimalLimit },
                    cancellationToken: System.Threading.CancellationToken.None).Result;

                ClassicAssert.AreEqual(1000, file.Chunks.OfType<PayloadCustomChunk>().Single().Values!.Length, "Async custom chunk payload length is invalid.");
            }
        }

        [Test]
        public void Read_MaxMemorySize_CustomChunk_DeclaredSizeExceedsLimit()
        {
            var chunkTypes = new ChunkTypesCollection { { typeof(PayloadCustomChunk), PayloadCustomChunk.Id } };
            var bytes = GetFileBytes(new PayloadCustomChunk(100000));

            AssertThrowsTooLarge(bytes, 1000, new ReadingSettings { CustomChunkTypes = chunkTypes });
        }

        [Test]
        public void Read_MaxMemorySize_MultipleChunks_SizesAreAccumulated()
        {
            var singleChunkBytes = GetFileBytes(new TrackChunk(CreateNotes(500)));
            var singleChunkLimit = GetMinimalMemoryLimit(singleChunkBytes);

            var twoChunksBytes = GetFileBytes(new TrackChunk(CreateNotes(500)), new TrackChunk(CreateNotes(500)));
            var twoChunksLimit = GetMinimalMemoryLimit(twoChunksBytes);

            ClassicAssert.GreaterOrEqual(twoChunksLimit, singleChunkLimit * 2 - 1000);
            AssertThrowsTooLarge(twoChunksBytes, singleChunkLimit);
        }

        [Test]
        public void Read_MaxMemorySize_EstimateIsProportionalToEventsCount()
        {
            var limit1000 = GetMinimalMemoryLimit(GetFileBytes(new TrackChunk(CreateNotes(1000))));
            var limit2000 = GetMinimalMemoryLimit(GetFileBytes(new TrackChunk(CreateNotes(2000))));

            ClassicAssert.AreEqual(2.0, (double)limit2000 / limit1000, 0.1);
        }

        [Test]
        public void Read_MaxMemorySize_EmptyTrackChunks_AreNotOverestimated()
        {
            Assume.That(IntPtr.Size, Is.EqualTo(8), "Sizes are pinned for 64-bit.");

            const int count = 10000;
            var bytes = GetFileBytes(Enumerable.Range(0, count).Select(_ => (MidiChunk)new TrackChunk()).ToArray(), null);

            // Chunk (32), events collection (40), list (32) and slot in the chunks list (8..16) per track chunk
            var perChunk = (double)GetMinimalMemoryLimit(bytes) / count;
            ClassicAssert.That(perChunk, Is.InRange(104.0, 128.0));
        }

        [Test]
        public void Read_MaxMemorySize_EventsEstimatedWithoutReflection_MatchDeepEstimation()
        {
            foreach (var midiEvent in GetEventsForMaxMemorySize())
            {
                // Strings of literals are interned and shared so they are not a part of the event's size
                var e = midiEvent is BaseTextEvent textEvent
                    ? (MidiEvent)Activator.CreateInstance(midiEvent.GetType())!
                    : midiEvent;
                if (e is BaseTextEvent baseTextEvent)
                    baseTextEvent.Text = new string('a', 10);

                ClassicAssert.AreEqual(
                    MemorySizeEstimator.EstimateDeep(e),
                    MemorySizeEstimator.EstimateEvent(e),
                    $"Fast estimation differs from the deep one for {e.GetType().Name}.");
            }
        }

        [Test]
        public void Read_MaxMemorySize_SharedObjectsAreNotCounted()
        {

            ClassicAssert.AreEqual(0, MemorySizeEstimator.EstimateString(0));
            ClassicAssert.AreEqual(0, MemorySizeEstimator.EstimateArray(typeof(byte), 0));
            ClassicAssert.AreEqual(0, MemorySizeEstimator.EstimateDeep(string.Empty));
            ClassicAssert.AreEqual(0, MemorySizeEstimator.EstimateDeep("MTrk"));
        }

        [Test]
        public void Read_MaxMemorySize_NoLimit()
        {
            var bytes = GetFileBytes(new TrackChunk(CreateNotes(1000)));
            var file = MidiFile.Read(new MemoryStream(bytes), new ReadingSettings { MaxMemorySize = null });
            ClassicAssert.AreEqual(2000, file.GetEvents().Count(), "Unlimited reading returned an invalid event count.");

            using (var stream = new MemoryStream(bytes))
            {
                file = MidiFile.ReadAsync(
                    stream,
                    new ReadingSettings { MaxMemorySize = null },
                    cancellationToken: System.Threading.CancellationToken.None).Result;

                ClassicAssert.AreEqual(2000, file.GetEvents().Count(), "Async unlimited reading returned an invalid event count.");
            }
        }

        [Test]
        public void Read_MaxMemorySize_EstimatedMemorySizeIsSet()
        {
            var bytes = GetFileBytes(new TrackChunk(CreateNotes(1000)));

            var file = MidiFile.Read(new MemoryStream(bytes), new ReadingSettings { MaxMemorySize = long.MaxValue });
            ClassicAssert.Greater(file.EstimatedMemorySize, 2000 * 32);

            using (var stream = new MemoryStream(bytes))
            {
                var asyncFile = MidiFile.ReadAsync(
                    stream,
                    new ReadingSettings { MaxMemorySize = long.MaxValue },
                    cancellationToken: System.Threading.CancellationToken.None).Result;

                ClassicAssert.Greater(asyncFile.EstimatedMemorySize, 2000 * 32, "Async estimated memory size is too small.");
                ClassicAssert.AreEqual(file.EstimatedMemorySize, asyncFile.EstimatedMemorySize, "Async estimated memory size differs.");
            }

            file = MidiFile.Read(new MemoryStream(bytes));
            ClassicAssert.AreEqual(0, file.EstimatedMemorySize, "Reading estimated memory size when no limit was requested.");

            using (var stream = new MemoryStream(bytes))
            {
                file = MidiFile.ReadAsync(
                    stream,
                    cancellationToken: System.Threading.CancellationToken.None).Result;

                ClassicAssert.AreEqual(0, file.EstimatedMemorySize, "Async reading estimated memory size when no limit was requested.");
            }
        }

        private static IEnumerable<MidiEvent> CreateNotes(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100);
                yield return new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0) { DeltaTime = 10 };
            }
        }

        private static byte[] GetFileBytes(params MidiChunk[] chunks) =>
            GetFileBytes(chunks, null);

        private static byte[] GetFileBytes(MidiChunk chunk, WritingSettings writingSettings) =>
            GetFileBytes(new[] { chunk }, writingSettings);

        internal static byte[] GetFileBytes(MidiChunk[] chunks, WritingSettings? writingSettings)
        {
            using (var stream = new MemoryStream())
            {
                new MidiFile(chunks).Write(stream, settings: writingSettings);
                return stream.ToArray();
            }
        }

        internal static long GetMinimalMemoryLimit(byte[] bytes, ReadingSettings? settings = null)
        {
            long low = 0;
            long high = 1L << 40;

            while (low < high)
            {
                var middle = low + (high - low) / 2;
                if (TryRead(bytes, middle, settings))
                    high = middle;
                else
                    low = middle + 1;
            }

            return low;
        }

        private static bool TryRead(byte[] bytes, long limit, ReadingSettings? settings)
        {
            bool expected;

            try
            {
                MidiFile.Read(new MemoryStream(bytes), CreateSettings(limit, settings));
                expected = true;
            }
            catch (MidiFileTooLargeException)
            {
                expected = false;
            }

            using (var stream = new MemoryStream(bytes))
            {
                if (expected)
                {
                    MidiFile.ReadAsync(
                        stream,
                        CreateSettings(limit, settings),
                        cancellationToken: System.Threading.CancellationToken.None).Wait();
                }
                else
                {
                    var aggregate = Assert.Throws<AggregateException>(() => MidiFile.ReadAsync(
                        stream,
                        CreateSettings(limit, settings),
                        cancellationToken: System.Threading.CancellationToken.None).Wait(), "Async reading did not reject the memory limit.");

                    ClassicAssert.IsInstanceOf<MidiFileTooLargeException>(aggregate!.InnerException, "Async memory-limit exception type differs.");
                }
            }

            return expected;
        }

        private static void AssertThrowsTooLarge(byte[] bytes, long limit, ReadingSettings? settings = null)
        {
            var ex = Assert.Throws<MidiFileTooLargeException>(
                () => MidiFile.Read(new MemoryStream(bytes), CreateSettings(limit, settings)));
            ClassicAssert.AreEqual(limit, ex!.MaxSize);
            ClassicAssert.Greater(ex.EstimatedSize, limit);

            using (var stream = new MemoryStream(bytes))
            {
                var aggregate = Assert.Throws<AggregateException>(() => MidiFile.ReadAsync(
                    stream,
                    CreateSettings(limit, settings),
                    cancellationToken: System.Threading.CancellationToken.None).Wait(), "Async oversized-file reading did not throw.");
                var asyncException = aggregate!.InnerException as MidiFileTooLargeException;

                ClassicAssert.IsNotNull(asyncException, "Async oversized-file exception type is invalid.");
                ClassicAssert.AreEqual(limit, asyncException!.MaxSize, "Async oversized-file exception reported an invalid limit.");
                ClassicAssert.Greater(asyncException.EstimatedSize, limit, "Async oversized-file exception did not report an exceeded estimate.");
            }
        }

        private static ReadingSettings CreateSettings(long limit, ReadingSettings? settings)
        {
            settings ??= new ReadingSettings();
            settings.MaxMemorySize = limit;
            return settings;
        }

        #endregion
    }
}
