using System.Collections.Generic;
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
        public void SplitByGrid_EmptyFile()
        {
            var midiFile = new MidiFile();
            var grid = new SteppedGrid(MusicalTimeSpan.Eighth);

            Assert.IsFalse(midiFile.SplitByGrid(grid).Any(),
                           "Empty file splitting produced non-empty result.");
        }

        [Test]
        public void SplitByGrid_DontSplitNotes_DontPreserveTimes()
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
            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(3, newFiles.Count, "New files count is invalid.");

            CompareTimedEvents(
                newFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(100000), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new SetTempoEvent(200000), 90),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90)
                },
                "First file contains invalid events.");

            CompareTimedEvents(
                newFiles[1].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(200000), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 50),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 100),
                },
                "Second file contains invalid events.");

            CompareTimedEvents(
                newFiles[2].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(200000), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                    new TimedEvent(new TextEvent("Test"), 0)
                },
                "Third file contains invalid events.");
        }

        [Test]
        public void SplitByGrid_ArbitraryGrid_DontSplitNotes_DontPreserveTimes()
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
            var grid = new ArbitraryGrid((MidiTimeSpan)100, (MidiTimeSpan)200);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(3, newFiles.Count, "New files count is invalid.");

            CompareTimedEvents(
                newFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(100000), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new SetTempoEvent(200000), 90),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90)
                },
                "First file contains invalid events.");

            CompareTimedEvents(
                newFiles[1].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(200000), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 50),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 100),
                },
                "Second file contains invalid events.");

            CompareTimedEvents(
                newFiles[2].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(200000), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                    new TimedEvent(new TextEvent("Test"), 0)
                },
                "Third file contains invalid events.");
        }

        [Test]
        public void SplitByGrid_DontSplitNotes_PreserveTimes()
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
            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = true
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(3, newFiles.Count, "New files count is invalid.");

            CompareTimedEvents(
                newFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new SetTempoEvent(200000), 90)
                },
                "First file contains invalid events.");

            CompareTimedEvents(
                newFiles[1].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 100),
                    new TimedEvent(new SetTempoEvent(200000), 100),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 150),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 200)
                },
                "Second file contains invalid events.");

            CompareTimedEvents(
                newFiles[2].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 200),
                    new TimedEvent(new SetTempoEvent(200000), 200),
                    new TimedEvent(new PitchBendEvent(1000), 200),
                    new TimedEvent(new TextEvent("Test"), 200)
                },
                "Third file contains invalid events.");
        }

        [Test]
        public void SplitByGrid_SplitNotes()
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
            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = true,
                PreserveTimes = false
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(2, newFiles.Count, "New files count is invalid.");

            CompareTimedEvents(
                newFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(100000), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 100)
                },
                "First file contains invalid events.");

            CompareTimedEvents(
                newFiles[1].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(100000), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 50),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 100)
                },
                "Second file contains invalid events.");
        }

        [Test]
        public void SplitByGrid_EmptyFiles()
        {
            var timedEvents = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 95),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 300),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 400)
            };

            var midiFile = timedEvents.ToFile();
            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(4, newFiles.Count, "New files count is invalid.");

            CompareTimedEvents(
                newFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 95),
                },
                "First file contains invalid events.");

            CompareTimedEvents(
                newFiles[1].GetTimedEvents(),
                Enumerable.Empty<TimedEvent>(),
                "Second file contains invalid events.");
            CompareTimedEvents(
                newFiles[2].GetTimedEvents(),
                Enumerable.Empty<TimedEvent>(),
                "Third file contains invalid events.");

            CompareTimedEvents(
                newFiles[3].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 0),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 100)
                },
                "Last file contains invalid events.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitByGrid_PreserveTrackChunks(bool preserveTrackChunks)
        {
            var timedEvents1 = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 95),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 100),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 200)
            };

            var timedEvents2 = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)21, (SevenBitNumber)100), 100),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)21, (SevenBitNumber)100), 200)
            };

            var midiFile = new MidiFile(
                timedEvents1.ToTrackChunk(),
                timedEvents2.ToTrackChunk());
            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false,
                PreserveTrackChunks = preserveTrackChunks
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(2, newFiles.Count, "New files count is invalid.");

            Assert.AreEqual(preserveTrackChunks ? 2 : 1, newFiles[0].GetTrackChunks().Count(), "Track chunks count of the first file is invalid.");
            CompareTimedEvents(
                newFiles[0].GetTrackChunks().First().GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 95),
                },
                "First track chunk of first file contains invalid events.");
            if (preserveTrackChunks)
                CompareTimedEvents(
                    newFiles[0].GetTrackChunks().Last().GetTimedEvents(),
                    Enumerable.Empty<TimedEvent>(),
                    "Second track chunk of first file contains invalid events.");

            Assert.AreEqual(2, newFiles[1].GetTrackChunks().Count(), "Track chunks count of the second file is invalid.");
            CompareTimedEvents(
                newFiles[1].GetTrackChunks().First().GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 0),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 100)
                },
                "First track chunk of second file contains invalid events.");
            CompareTimedEvents(
                newFiles[1].GetTrackChunks().Last().GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)21, (SevenBitNumber)100), 0),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)21, (SevenBitNumber)100), 100)
                },
                "Second track chunk of second file contains invalid events.");
        }

        #endregion
    }
}
