using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        }

        #endregion

        #region Test methods

        private static IEnumerable<MidiEvent> GetEventsForMaxMemorySize()
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
            ClassicAssert.AreEqual(1000, file.Chunks.OfType<PayloadCustomChunk>().Single().Values!.Length);
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
        public void Read_MaxMemorySize_EstimateIsCloseToRealMemoryUsage()
        {
            var bytes = GetFileBytes(new TrackChunk(CreateNotes(100000)));
            var estimate = GetMinimalMemoryLimit(bytes);

            var before = GC.GetTotalMemory(true);
            var file = MidiFile.Read(new MemoryStream(bytes));
            var after = GC.GetTotalMemory(true);
            GC.KeepAlive(file);

            var actual = after - before;
            ClassicAssert.Greater(estimate, actual * 0.5, $"Estimate ({estimate}) is too small compared to actual ({actual}).");
            ClassicAssert.Less(estimate, actual * 2, $"Estimate ({estimate}) is too big compared to actual ({actual}).");
        }

        [Test]
        public void Read_MaxMemorySize_NoLimit()
        {
            var bytes = GetFileBytes(new TrackChunk(CreateNotes(1000)));
            var file = MidiFile.Read(new MemoryStream(bytes), new ReadingSettings { MaxMemorySize = null });
            ClassicAssert.AreEqual(2000, file.GetEvents().Count());
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

        private static byte[] GetFileBytes(MidiChunk[] chunks, WritingSettings? writingSettings)
        {
            using (var stream = new MemoryStream())
            {
                new MidiFile(chunks).Write(stream, settings: writingSettings);
                return stream.ToArray();
            }
        }

        private static long GetMinimalMemoryLimit(byte[] bytes, ReadingSettings? settings = null)
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
            try
            {
                MidiFile.Read(new MemoryStream(bytes), CreateSettings(limit, settings));
                return true;
            }
            catch (MidiFileTooLargeException)
            {
                return false;
            }
        }

        private static void AssertThrowsTooLarge(byte[] bytes, long limit, ReadingSettings? settings = null)
        {
            var ex = Assert.Throws<MidiFileTooLargeException>(
                () => MidiFile.Read(new MemoryStream(bytes), CreateSettings(limit, settings)));
            ClassicAssert.AreEqual(limit, ex!.MaxSize);
            ClassicAssert.Greater(ex.EstimatedSize, limit);
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
