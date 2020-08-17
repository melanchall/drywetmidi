using System.Linq;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

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

            #endregion
        }

        #endregion

        #region Test methods

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
            Assert.AreEqual(string.Empty, textEvent.Text, "Text is not an empty string.");
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
            Assert.IsNull(textEvent.Text, "Text is not null.");
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
            Assert.IsNull(sequencerSpecificEvent.Data, "Data is not null.");
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
            Assert.IsNull(unknownMetaEvent.Data, "Data is not null.");
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
            Assert.IsNull(unknownChunk.Data, "Data is not null.");
        }

        #endregion
    }
}
