using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Nested classes

        private sealed class EmptyCustomMetaEvent : MetaEvent
        {
            #region Constructor

            public EmptyCustomMetaEvent()
            {
            }

            #endregion

            #region Overrides

            protected override MidiEvent CloneEvent()
            {
                return new EmptyCustomMetaEvent();
            }

            protected override int GetContentSize(WritingSettings settings)
            {
                return 0;
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
            {
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
            }

            protected override Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                int size,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            protected override Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            #endregion
        }

        private sealed class EmptyCustomChunk : MidiChunk
        {
            #region Constants

            public const string Id = "Cstm";

            #endregion

            #region Constructor

            public EmptyCustomChunk()
                : base(Id)
            {
            }

            #endregion

            #region Overrides

            public override MidiChunk Clone()
            {
                return new EmptyCustomChunk();
            }

            protected override uint GetContentSize(WritingSettings settings)
            {
                return 0;
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
            }

            protected override Task ReadContentAsync(
                MidiReader reader,
                ReadingSettings settings,
                uint size,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ReadContent(reader, settings, size);
                return Task.CompletedTask;
            }

            protected override Task WriteContentAsync(
                MidiWriter writer,
                WritingSettings settings,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                WriteContent(writer, settings);
                return Task.CompletedTask;
            }

            #endregion
        }

        #endregion

        #region Test methods

        [Test]
        public async Task ReadWriteEmptyCustomChunk_CompletedTaskDelegation()
        {
            using var stream = new MemoryStream();
            var file = new MidiFile(new EmptyCustomChunk());
            var settings = new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection { { typeof(EmptyCustomChunk), EmptyCustomChunk.Id } }
            };

            await file.WriteAsync(stream);
            stream.Position = 0;
            var actual = await MidiFile.ReadAsync(stream, settings);

            ClassicAssert.AreEqual(1, actual.Chunks.Count, "Reading changed the number of empty custom chunks.");
            ClassicAssert.IsInstanceOf<EmptyCustomChunk>(actual.Chunks[0], "Reading did not use the empty custom chunk implementation.");
            ClassicAssert.AreEqual(stream.Length, stream.Position, "Reading did not consume the empty custom chunk header.");
            ClassicAssert.IsTrue(stream.CanRead, "Reading disposed the caller-owned stream.");
        }

        [Test]
        public void ReadZeroLengthData_TextEvent_ReadAsEmptyObject()
        {
            var midiFile = WriteRead(
                new MidiFile(new TrackChunk(new TextEvent())),
                readingSettings: new ReadingSettings
                {
                    ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsEmptyObject
                });

            var textEvent = midiFile.GetEvents().OfType<TextEvent>().Single();
            ClassicAssert.AreEqual(string.Empty, textEvent.Text, "Text is not an empty string.");
        }

        [Test]
        public void ReadZeroLengthData_TextEvent_ReadAsNull()
        {
            var midiFile = WriteRead(
                new MidiFile(new TrackChunk(new TextEvent())),
                readingSettings: new ReadingSettings
                {
                    ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsNull
                });

            var textEvent = midiFile.GetEvents().OfType<TextEvent>().Single();
            ClassicAssert.IsNull(textEvent.Text, "Text is not null.");
        }

        [Test]
        public void ReadZeroLengthData_SequencerSpecificEvent_ReadAsEmptyObject()
        {
            var midiFile = WriteRead(
                new MidiFile(new TrackChunk(new SequencerSpecificEvent())),
                readingSettings: new ReadingSettings
                {
                    ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsEmptyObject
                });

            var sequencerSpecificEvent = midiFile.GetEvents().OfType<SequencerSpecificEvent>().Single();
            CollectionAssert.AreEqual(new byte[0], sequencerSpecificEvent.Data, "Data is not a zero-sized array.");
        }

        [Test]
        public void ReadZeroLengthData_SequencerSpecificEvent_ReadAsNull()
        {
            var midiFile = WriteRead(
                new MidiFile(new TrackChunk(new SequencerSpecificEvent())),
                readingSettings: new ReadingSettings
                {
                    ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsNull
                });

            var sequencerSpecificEvent = midiFile.GetEvents().OfType<SequencerSpecificEvent>().Single();
            ClassicAssert.IsNull(sequencerSpecificEvent.Data, "Data is not null.");
        }

        [Test]
        public void ReadZeroLengthData_UnknownMetaEvent_ReadAsEmptyObject()
        {
            var midiFile = WriteRead(
                new MidiFile(new TrackChunk(new EmptyCustomMetaEvent())),
                writingSettings: new WritingSettings
                {
                    CustomMetaEventTypes = new EventTypesCollection
                    {
                        { typeof(EmptyCustomMetaEvent), 0x5A }
                    }
                },
                readingSettings: new ReadingSettings
                {
                    ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsEmptyObject
                });

            var unknownMetaEvent = midiFile.GetEvents().OfType<UnknownMetaEvent>().Single();
            CollectionAssert.AreEqual(new byte[0], unknownMetaEvent.Data, "Data is not a zero-sized array.");
        }

        [Test]
        public void ReadZeroLengthData_UnknownMetaEvent_ReadAsNull()
        {
            var midiFile = WriteRead(
                new MidiFile(new TrackChunk(new EmptyCustomMetaEvent())),
                writingSettings: new WritingSettings
                {
                    CustomMetaEventTypes = new EventTypesCollection
                    {
                        { typeof(EmptyCustomMetaEvent), 0x5A }
                    }
                },
                readingSettings: new ReadingSettings
                {
                    ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsNull
                });

            var unknownMetaEvent = midiFile.GetEvents().OfType<UnknownMetaEvent>().Single();
            ClassicAssert.IsNull(unknownMetaEvent.Data, "Data is not null.");
        }

        [Test]
        public void ReadZeroLengthData_UnknownChunk_ReadAsEmptyObject()
        {
            var midiFile = WriteRead(
                new MidiFile(new EmptyCustomChunk()),
                readingSettings: new ReadingSettings
                {
                    ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsEmptyObject
                });

            var unknownChunk = midiFile.Chunks.OfType<UnknownChunk>().Single();
            CollectionAssert.AreEqual(new byte[0], unknownChunk.Data, "Data is not a zero-sized array.");
        }

        [Test]
        public void ReadZeroLengthData_UnknownChunk_ReadAsNull()
        {
            var midiFile = WriteRead(
                new MidiFile(new EmptyCustomChunk()),
                readingSettings: new ReadingSettings
                {
                    ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsNull
                });

            var unknownChunk = midiFile.Chunks.OfType<UnknownChunk>().Single();
            ClassicAssert.IsNull(unknownChunk.Data, "Data is not null.");
        }

        #endregion
    }
}
