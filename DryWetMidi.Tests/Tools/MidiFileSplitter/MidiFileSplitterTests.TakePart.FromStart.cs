using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class MidiFileSplitterTests
    {
        #region Test methods

        [Test]
        public void TakePart_FromStart_EmptyFile()
        {
            var midiFile = new MidiFile();
            var result = midiFile.TakePart(MusicalTimeSpan.Eighth);

            Assert.IsTrue(result.IsEmpty(), "Empty file part taking produced non-empty result.");
        }

        [Test]
        public void TakePart_FromStart_DontSplitNotes_DontPreserveTimes()
        {
            var timedEvents = new[]
            {
                new TimedEvent(new SetTempoEvent(100000), 0),
                new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                new TimedEvent(new SetTempoEvent(200000), 90),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 150),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)100), 190),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 200),
                new TimedEvent(new TextEvent("Test"), 200)
            };

            var midiFile = timedEvents.ToFile();
            var partLength = (MidiTimeSpan)100;
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false
            };

            var result = midiFile.TakePart(partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(100000), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new SetTempoEvent(200000), 90),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90)
                },
                "Resulting file contains invalid events.");
        }

        [Test]
        public void TakePart_FromStart_DontSplitNotes_PreserveTimes()
        {
            var timedEvents = new[]
            {
                new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                new TimedEvent(new SetTempoEvent(200000), 90),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 150),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 200),
                new TimedEvent(new PitchBendEvent(1000), 200),
                new TimedEvent(new TextEvent("Test"), 200)
            };

            var midiFile = timedEvents.ToFile();
            var partLength = (MidiTimeSpan)100;
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = true
            };

            var result = midiFile.TakePart(partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new SetTempoEvent(200000), 90)
                },
                "Resulting file contains invalid events.");
        }

        [Test]
        public void TakePart_FromStart_SplitNotes()
        {
            var timedEvents = new[]
            {
                new TimedEvent(new SetTempoEvent(100000), 0),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 150),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 190),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 200)
            };

            var midiFile = timedEvents.ToFile();
            var partLength = (MidiTimeSpan)100;
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = true,
                PreserveTimes = false
            };

            var result = midiFile.TakePart(partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(100000), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 100)
                },
                "Resulting file contains invalid events.");
        }

        [Test]
        public void TakePart_FromStart_EmptyFiles()
        {
            var timedEvents = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 300),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 400)
            };

            var midiFile = timedEvents.ToFile();
            var partLength = (MidiTimeSpan)100;
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false
            };

            var result = midiFile.TakePart(partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                Enumerable.Empty<TimedEvent>(),
                "Resulting file contains invalid events.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TakePart_FromStart_PreserveTrackChunks(bool preserveTrackChunks)
        {
            var timedEvents1 = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 95),
            };

            var timedEvents2 = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)21, (SevenBitNumber)100), 100),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)21, (SevenBitNumber)100), 200)
            };

            var midiFile = new MidiFile(timedEvents1.ToTrackChunk(), timedEvents2.ToTrackChunk());
            var partLength = (MidiTimeSpan)100;
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false,
                PreserveTrackChunks = preserveTrackChunks
            };

            var result = midiFile.TakePart(partLength, settings);
            Assert.AreEqual(preserveTrackChunks ? 2 : 1, result.GetTrackChunks().Count(), "Track chunks count of resulting file is invalid.");

            CompareTimedEvents(
                result.GetTrackChunks().First().GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 95),
                },
                "First track chunk of resulting file contains invalid events.");
            if (preserveTrackChunks)
            {
                CompareTimedEvents(
                    result.GetTrackChunks().Last().GetTimedEvents(),
                    Enumerable.Empty<TimedEvent>(),
                    "Second track chunk of resulting file contains invalid events.");
            }
        }

        #endregion
    }
}
