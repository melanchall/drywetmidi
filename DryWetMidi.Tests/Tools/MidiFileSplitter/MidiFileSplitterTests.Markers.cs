using System.Collections.Generic;
using System.Linq;
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

        [TestCase(0)]
        [TestCase(5)]
        public void PartStartMarkerEventFactory_PreserveTimes(long firstEventTime)
        {
            var timedEvents = new[]
            {
                new TimedEvent(new TextEvent("A"), firstEventTime),
                new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                new TimedEvent(new TextEvent("B"), 90),
                new TimedEvent(new TextEvent("C"), 210)
            };

            var midiFile = timedEvents.ToFile();
            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = true,
                Markers = new SliceMidiFileMarkers
                {
                    PartStartMarkerEventFactory = () => new MarkerEvent("X")
                }
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(3, newFiles.Count, "New files count is invalid.");

            CompareTimedEvents(
                newFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("X"), 0),
                    new TimedEvent(new TextEvent("A"), firstEventTime),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new TextEvent("B"), 90)
                },
                "First file contains invalid events.");

            CompareTimedEvents(
                newFiles[1].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("X"), 100),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 100),
                },
                "Second file contains invalid events.");

            CompareTimedEvents(
                newFiles[2].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("X"), 200),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 200),
                    new TimedEvent(new TextEvent("C"), 210)
                },
                "Third file contains invalid events.");
        }

        [TestCase(0)]
        [TestCase(5)]
        public void PartStartMarkerEventFactory_DontPreserveTimes(long firstEventTime)
        {
            var timedEvents = new[]
            {
                new TimedEvent(new TextEvent("A"), firstEventTime),
                new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                new TimedEvent(new TextEvent("B"), 90),
                new TimedEvent(new TextEvent("C"), 210)
            };

            var midiFile = timedEvents.ToFile();
            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false,
                Markers = new SliceMidiFileMarkers
                {
                    PartStartMarkerEventFactory = () => new MarkerEvent("X")
                }
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(3, newFiles.Count, "New files count is invalid.");

            CompareTimedEvents(
                newFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("X"), 0),
                    new TimedEvent(new TextEvent("A"), firstEventTime),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new TextEvent("B"), 90)
                },
                "First file contains invalid events.");

            CompareTimedEvents(
                newFiles[1].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("X"), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                },
                "Second file contains invalid events.");

            CompareTimedEvents(
                newFiles[2].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("X"), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                    new TimedEvent(new TextEvent("C"), 10)
                },
                "Third file contains invalid events.");
        }

        [Test]
        public void PartEndMarkerEventFactory_PreserveTimes()
        {
            var timedEvents = new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                new TimedEvent(new TextEvent("B"), 90),
                new TimedEvent(new TextEvent("C"), 210)
            };

            var midiFile = timedEvents.ToFile();
            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = true,
                Markers = new SliceMidiFileMarkers
                {
                    PartEndMarkerEventFactory = () => new MarkerEvent("X")
                }
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(3, newFiles.Count, "New files count is invalid.");

            CompareTimedEvents(
                newFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new TextEvent("B"), 90),
                    new TimedEvent(new MarkerEvent("X"), 100)
                },
                "First file contains invalid events.");

            CompareTimedEvents(
                newFiles[1].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 100),
                    new TimedEvent(new MarkerEvent("X"), 200),
                },
                "Second file contains invalid events.");

            CompareTimedEvents(
                newFiles[2].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 200),
                    new TimedEvent(new TextEvent("C"), 210),
                    new TimedEvent(new MarkerEvent("X"), 300)
                },
                "Third file contains invalid events.");
        }

        [Test]
        public void PartEndMarkerEventFactory_DontPreserveTimes()
        {
            var timedEvents = new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                new TimedEvent(new TextEvent("B"), 90),
                new TimedEvent(new TextEvent("C"), 210)
            };

            var midiFile = timedEvents.ToFile();
            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false,
                Markers = new SliceMidiFileMarkers
                {
                    PartEndMarkerEventFactory = () => new MarkerEvent("X")
                }
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(3, newFiles.Count, "New files count is invalid.");

            CompareTimedEvents(
                newFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new TextEvent("B"), 90),
                    new TimedEvent(new MarkerEvent("X"), 100)
                },
                "First file contains invalid events.");

            CompareTimedEvents(
                newFiles[1].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                    new TimedEvent(new MarkerEvent("X"), 100)
                },
                "Second file contains invalid events.");

            CompareTimedEvents(
                newFiles[2].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                    new TimedEvent(new TextEvent("C"), 10),
                    new TimedEvent(new MarkerEvent("X"), 100)
                },
                "Third file contains invalid events.");
        }

        [TestCase(0)]
        [TestCase(5)]
        public void PartStartMarkerEventFactory_PartEndMarkerEventFactory_PreserveTimes(long firstEventTime)
        {
            var timedEvents = new[]
            {
                new TimedEvent(new TextEvent("A"), firstEventTime),
                new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                new TimedEvent(new TextEvent("B"), 90),
                new TimedEvent(new TextEvent("C"), 210)
            };

            var midiFile = timedEvents.ToFile();
            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = true,
                Markers = new SliceMidiFileMarkers
                {
                    PartStartMarkerEventFactory = () => new MarkerEvent("S"),
                    PartEndMarkerEventFactory = () => new MarkerEvent("E")
                }
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(3, newFiles.Count, "New files count is invalid.");

            CompareTimedEvents(
                newFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("S"), 0),
                    new TimedEvent(new TextEvent("A"), firstEventTime),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new TextEvent("B"), 90),
                    new TimedEvent(new MarkerEvent("E"), 100)
                },
                "First file contains invalid events.");

            CompareTimedEvents(
                newFiles[1].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("S"), 100),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 100),
                    new TimedEvent(new MarkerEvent("E"), 200)
                },
                "Second file contains invalid events.");

            CompareTimedEvents(
                newFiles[2].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("S"), 200),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 200),
                    new TimedEvent(new TextEvent("C"), 210),
                    new TimedEvent(new MarkerEvent("E"), 300)
                },
                "Third file contains invalid events.");
        }

        [TestCase(0)]
        [TestCase(5)]
        public void PartStartMarkerEventFactory_PartEndMarkerEventFactory_DontPreserveTimes(long firstEventTime)
        {
            var timedEvents = new[]
            {
                new TimedEvent(new TextEvent("A"), firstEventTime),
                new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                new TimedEvent(new TextEvent("B"), 90),
                new TimedEvent(new TextEvent("C"), 210)
            };

            var midiFile = timedEvents.ToFile();
            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false,
                Markers = new SliceMidiFileMarkers
                {
                    PartStartMarkerEventFactory = () => new MarkerEvent("S"),
                    PartEndMarkerEventFactory = () => new MarkerEvent("E")
                }
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(3, newFiles.Count, "New files count is invalid.");

            CompareTimedEvents(
                newFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("S"), 0),
                    new TimedEvent(new TextEvent("A"), firstEventTime),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new TextEvent("B"), 90),
                    new TimedEvent(new MarkerEvent("E"), 100)
                },
                "First file contains invalid events.");

            CompareTimedEvents(
                newFiles[1].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("S"), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                    new TimedEvent(new MarkerEvent("E"), 100)
                },
                "Second file contains invalid events.");

            CompareTimedEvents(
                newFiles[2].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new MarkerEvent("S"), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                    new TimedEvent(new TextEvent("C"), 10),
                    new TimedEvent(new MarkerEvent("E"), 100)
                },
                "Third file contains invalid events.");
        }

        [Test]
        public void SplitByGridAndRemoveEmptyPartsAndMergeIntoNewFile()
        {
            var timedEvents1 = new[]
            {
                new TimedEvent(new TextEvent("A1"), 0),
                new TimedEvent(new SetTempoEvent(100000), 10),
                new TimedEvent(new TextEvent("B1"), 90),
                new TimedEvent(new TextEvent("C1"), 210),
                new TimedEvent(new NoteOnEvent(), 270),
                new TimedEvent(new NoteOffEvent(), 320)
            };

            var timedEvents2 = new[]
            {
                new TimedEvent(new TextEvent("A2"), 10),
                new TimedEvent(new TextEvent("B2"), 70),
                new TimedEvent(new TextEvent("C2"), 260)
            };

            var trackChunk1 = timedEvents1.ToTrackChunk();
            var trackChunk2 = timedEvents2.ToTrackChunk();
            var midiFile = new MidiFile(trackChunk1, trackChunk2);

            var grid = new SteppedGrid((MidiTimeSpan)100);
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false,
                PreserveTrackChunks = true,
                Markers = new SliceMidiFileMarkers
                {
                    PartStartMarkerEventFactory = () => new MarkerEvent("S"),
                    PartEndMarkerEventFactory = () => new MarkerEvent("F"),
                    EmptyPartMarkerEventFactory = () => new MarkerEvent("E")
                }
            };

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(4, newFiles.Count, "New files count is invalid.");

            //

            var nonEmptyParts = newFiles.Where(f =>
            {
                var newTimedEvents = f.GetTimedEvents().ToArray();
                return newTimedEvents.Any() && !newTimedEvents.Any(e => MidiEvent.Equals(e.Event, new MarkerEvent("E")));
            })
                .ToArray();
            Assert.AreEqual(3, nonEmptyParts.Length, "Non-empty new files count is invalid.");

            //

            var trackChunksCount = midiFile.GetTrackChunks().Count();
            var newTrackChunks = new List<TrackChunk>();

            for (var i = 0; i < trackChunksCount; i++)
            {
                var trackChunk = new TrackChunk();

                foreach (var part in nonEmptyParts)
                {
                    trackChunk.Events.AddRange(part.GetTrackChunks().ElementAt(i).Events);
                }

                newTrackChunks.Add(trackChunk);
            }

            var resultFile = new MidiFile(newTrackChunks)
            {
                TimeDivision = midiFile.TimeDivision
            };

            //

            var equalityCheckSettings = new MidiEventEqualityCheckSettings
            {
                CompareDeltaTimes = false
            };

            resultFile.RemoveTimedEvents(e =>
                MidiEvent.Equals(e.Event, new MarkerEvent("S"), equalityCheckSettings) ||
                MidiEvent.Equals(e.Event, new MarkerEvent("F"), equalityCheckSettings) ||
                MidiEvent.Equals(e.Event, new MarkerEvent("E"), equalityCheckSettings));

            //

            CompareTimedEvents(
                resultFile.GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new TextEvent("A1"), 0),
                    new TimedEvent(new SetTempoEvent(100000), 10),
                    new TimedEvent(new TextEvent("A2"), 10),
                    new TimedEvent(new TextEvent("B2"), 70),
                    new TimedEvent(new TextEvent("B1"), 90),
                    new TimedEvent(new SetTempoEvent(100000), 100),
                    new TimedEvent(new TextEvent("C1"), 110),
                    new TimedEvent(new TextEvent("C2"), 160),
                    new TimedEvent(new NoteOnEvent(), 170),
                    new TimedEvent(new SetTempoEvent(100000), 200),
                    new TimedEvent(new NoteOffEvent(), 220)
                },
                "Result file contains invalid events.");
        }

        #endregion
    }
}
