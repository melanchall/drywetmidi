using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Nested classes

        private sealed class CustomMetaEvent : MetaEvent
        {
            #region Constructor

            public CustomMetaEvent()
                : base()
            {
            }

            public CustomMetaEvent(int a, string b, byte c)
                : this()
            {
                A = a;
                B = b;
                C = c;
            }

            #endregion

            #region Properties

            public int A { get; private set; }

            public string B { get; private set; }

            public byte C { get; private set; }

            #endregion

            #region Overrides

            protected override MidiEvent CloneEvent()
            {
                return new CustomMetaEvent(A, B, C);
            }

            protected override int GetContentSize(WritingSettings settings)
            {
                return DataTypesUtilities.GetVlqLength(A) +
                       DataTypesUtilities.GetVlqLength(B?.Length ?? 0) +
                       (B?.Length ?? 0) +
                       1;
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
            {
                A = reader.ReadVlqNumber();

                var bLength = reader.ReadVlqNumber();
                B = reader.ReadString(bLength);

                C = reader.ReadByte();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                writer.WriteVlqNumber(A);
                writer.WriteVlqNumber(B?.Length ?? 0);
                writer.WriteString(B);
                writer.WriteByte(C);
            }

            #endregion
        }

        private sealed class CustomChunk : MidiChunk
        {
            #region Constants

            public const string Id = "Cstm";

            #endregion

            #region Constructor

            public CustomChunk()
                : base(Id)
            {
            }

            public CustomChunk(int a, string b, byte c)
                : this()
            {
                A = a;
                B = b;
                C = c;
            }

            #endregion

            #region Properties

            public int A { get; private set; }

            public string B { get; private set; }

            public byte C { get; private set; }

            #endregion

            #region Overrides

            public override MidiChunk Clone()
            {
                return new CustomChunk(A, B, C);
            }

            protected override uint GetContentSize(WritingSettings settings)
            {
                return (uint)(DataTypesUtilities.GetVlqLength(A) +
                              DataTypesUtilities.GetVlqLength(B?.Length ?? 0) +
                              (B?.Length ?? 0) +
                              1);
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                A = reader.ReadVlqNumber();

                var bLength = reader.ReadVlqNumber();
                B = reader.ReadString(bLength);

                C = reader.ReadByte();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                writer.WriteVlqNumber(A);
                writer.WriteVlqNumber(B?.Length ?? 0);
                writer.WriteString(B);
                writer.WriteByte(C);
            }

            public override bool Equals(object obj) => obj is CustomChunk customChunk &&
                customChunk.A == A &&
                customChunk.B == B &&
                customChunk.C == C;

            public override int GetHashCode()
            {
                unchecked
                {
                    var result = 17;
                    result = result * 23 + A.GetHashCode();
                    result = result * 23 + B.GetHashCode();
                    result = result * 23 + C.GetHashCode();
                    return result;
                }
            }

            #endregion
        }

        #endregion

        #region Test methods

        [Test]
        public void ReadWriteCustomMetaEvent()
        {
            const int expectedA = 1234567;
            const string expectedB = "Test";
            const byte expectedC = 45;

            var customMetaEventTypes = new EventTypesCollection
            {
                { typeof(CustomMetaEvent), 0x5A }
            };

            var writingSettings = new WritingSettings { CustomMetaEventTypes = customMetaEventTypes };
            var readingSettings = new ReadingSettings { CustomMetaEventTypes = customMetaEventTypes };

            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(
                    new TrackChunk(
                        new CustomMetaEvent(expectedA, expectedB, expectedC) { DeltaTime = 100 },
                        new TextEvent("foo"),
                        new MarkerEvent("bar"))),
                writingSettings,
                readingSettings);

            var customMetaEvents = midiFile.GetEvents().OfType<CustomMetaEvent>().ToArray();
            ClassicAssert.AreEqual(1, customMetaEvents.Length, "Custom meta events count is invalid.");

            var customMetaEvent = customMetaEvents.Single();
            ClassicAssert.AreEqual(100, customMetaEvent.DeltaTime, "Delta-time is invalid.");
            ClassicAssert.AreEqual(expectedA, customMetaEvent.A, "A value is invalid");
            ClassicAssert.AreEqual(expectedB, customMetaEvent.B, "B value is invalid");
            ClassicAssert.AreEqual(expectedC, customMetaEvent.C, "C value is invalid");
        }

        [Test]
        public void ReadWriteCustomChunk()
        {
            const int expectedA = 1234567;
            const string expectedB = "Test";
            const byte expectedC = 45;

            var customChunkTypes = new ChunkTypesCollection
            {
                { typeof(CustomChunk), CustomChunk.Id }
            };

            var readingSettings = new ReadingSettings { CustomChunkTypes = customChunkTypes };

            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("foo"),
                        new MarkerEvent("bar")),
                    new CustomChunk(expectedA, expectedB, expectedC)),
                null,
                readingSettings);

            var customChunks = midiFile.Chunks.OfType<CustomChunk>().ToArray();
            ClassicAssert.AreEqual(1, customChunks.Length, "Custom chunks count is invalid.");

            var customChunk = customChunks.Single();
            ClassicAssert.AreEqual(expectedA, customChunk.A, "A value is invalid");
            ClassicAssert.AreEqual(expectedB, customChunk.B, "B value is invalid");
            ClassicAssert.AreEqual(expectedC, customChunk.C, "C value is invalid");
        }

        [Test]
        public void ReadWriteCustomChunk_NoTypesProvided()
        {
            const int expectedA = 1234567;
            const string expectedB = "Test";
            const byte expectedC = 45;

            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(
                    new TrackChunk(
                        new TextEvent("foo"),
                        new MarkerEvent("bar")),
                    new CustomChunk(expectedA, expectedB, expectedC)),
                null,
                null);

            var customChunks = midiFile.Chunks.OfType<CustomChunk>().ToArray();
            CollectionAssert.IsEmpty(customChunks, "Custom chunks are read.");

            var unknownChunks = midiFile.Chunks.OfType<UnknownChunk>().ToArray();
            ClassicAssert.AreEqual(1, unknownChunks.Length, "Unknown chunks count is invalid.");

            var unknownChunk = unknownChunks.Single();
            ClassicAssert.AreEqual(CustomChunk.Id, unknownChunk.ChunkId, "Unknown chunk ID is invalid.");

            var expectedBytesCount =
                expectedA.GetVlqLength() +
                expectedB.Length.GetVlqLength() +
                expectedB.Length +
                1;
            ClassicAssert.AreEqual(expectedBytesCount, unknownChunk.Data.Length, "Unknown chunk data length is invalid.");
        }

        #endregion
    }
}
