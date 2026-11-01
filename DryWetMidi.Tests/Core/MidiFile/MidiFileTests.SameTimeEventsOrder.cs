using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Core
{
    // Order of same-time control changes carries meaning (RPN selectors, repeated values, etc.),
    // so it must survive writing and managing timed events.
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Test methods

        [Test]
        public void Write_SameTimeControlChanges_OrderIsPreserved()
        {
            var expected = GetSameTimeControlChanges();

            var trackChunk = new TrackChunk(expected.Select(e => (MidiEvent)e.Clone()));
            var readFile = WriteAndRead(new MidiFile(trackChunk));

            CheckControlChangesOrder(readFile.GetTrackChunks().First(), expected);
        }

        [Test]
        public void SaveChanges_SameTimeControlChanges_OrderIsPreserved()
        {
            var expected = GetSameTimeControlChanges();

            var trackChunk = new TrackChunk();
            using (var manager = trackChunk.ManageTimedEvents())
            {
                manager.Objects.Add(new TimedEvent(expected[0].Clone(), 10));
                manager.Objects.Add(new TimedEvent(expected[1].Clone(), 10));
                manager.Objects.Add(new TimedEvent(expected[2].Clone(), 10));
                manager.Objects.Add(new TimedEvent(expected[3].Clone(), 10));
                manager.Objects.Add(new TimedEvent(expected[4].Clone(), 10));
                manager.Objects.Add(new TimedEvent(expected[5].Clone(), 10));
            }

            var readFile = WriteAndRead(new MidiFile(trackChunk));

            CheckControlChangesOrder(readFile.GetTrackChunks().First(), expected);
        }

        #endregion

        #region Private methods

        private static ControlChangeEvent[] GetSameTimeControlChanges()
        {
            return new[]
            {
                new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)100),
                new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)40),
                new ControlChangeEvent((SevenBitNumber)101, (SevenBitNumber)0),
                new ControlChangeEvent((SevenBitNumber)100, (SevenBitNumber)0),
                new ControlChangeEvent((SevenBitNumber)6, (SevenBitNumber)2),
                new ControlChangeEvent((SevenBitNumber)38, (SevenBitNumber)10)
            };
        }

        private static MidiFile WriteAndRead(MidiFile midiFile)
        {
            using (var stream = new MemoryStream())
            {
                midiFile.Write(stream);
                stream.Position = 0;

                return MidiFile.Read(stream);
            }
        }

        private static void CheckControlChangesOrder(
            TrackChunk trackChunk,
            ControlChangeEvent[] expected)
        {
            var actual = trackChunk
                .Events
                .OfType<ControlChangeEvent>()
                .Select(e => (e.ControlNumber, e.ControlValue))
                .ToArray();

            CollectionAssert.AreEqual(
                expected.Select(e => (e.ControlNumber, e.ControlValue)).ToArray(),
                actual);
        }

        #endregion
    }
}
