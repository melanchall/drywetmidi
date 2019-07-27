using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class MidiFileSplitterTests
    {
        #region Constants

        private static readonly NoteMethods NoteMethods = new NoteMethods();

        #endregion

        #region Test methods

        #region SplitByChannel

        [Test]
        public void SplitByChannel_ValidFiles()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile = MidiFile.Read(filePath);
                var originalChannels = midiFile.GetTrackChunks()
                                               .SelectMany(c => c.Events)
                                               .OfType<ChannelEvent>()
                                               .Select(e => e.Channel)
                                               .Distinct()
                                               .ToArray();

                var filesByChannel = midiFile.SplitByChannel().ToList();
                var allChannels = new List<FourBitNumber>(FourBitNumber.Values.Length);

                foreach (var fileByChannel in filesByChannel)
                {
                    Assert.AreEqual(fileByChannel.TimeDivision,
                                    midiFile.TimeDivision,
                                    "Time division of new file doesn't equal to the time division of the original one.");

                    var channels = fileByChannel.GetTrackChunks()
                                                .SelectMany(c => c.Events)
                                                .OfType<ChannelEvent>()
                                                .Select(e => e.Channel)
                                                .Distinct()
                                                .ToArray();
                    Assert.AreEqual(1,
                                    channels.Length,
                                    "New file contains channel events for different channels.");

                    allChannels.Add(channels.First());
                }

                allChannels.Sort();

                CollectionAssert.AreEqual(originalChannels.OrderBy(c => c),
                                          allChannels,
                                          "Channels from new files differs from those from original one.");
            }
        }

        #endregion

        #region SplitByNotes

        [Test]
        public void SplitByNotes_EmptyFile()
        {
            var midiFile = new MidiFile();

            Assert.IsFalse(midiFile.SplitByNotes().Any(), "Empty file splitting produced non-empty result.");
        }

        [Test]
        public void SplitByNotes_NoNoteEvents()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new SetTempoEvent(100000),
                    new TextEvent()));

            Assert.IsFalse(midiFile.SplitByNotes().Any(), "Empty file splitting produced non-empty result.");
        }

        [Test]
        public void SplitByNotes_SingleChannel()
        {
            var tempoMap = TempoMap.Default;

            var midiFile = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Quarter)
                .SetOctave(2)

                .Note(NoteName.A)

                .SetProgram((SevenBitNumber)20)
                .Note(NoteName.C)

                .Build()
                .ToFile(tempoMap);

            var filesByNotes = midiFile.SplitByNotes().ToList();
            Assert.AreEqual(2, filesByNotes.Count, "New files count is invalid.");

            var notes = midiFile.GetNotes().ToList();

            Assert.IsTrue(NoteEquality.AreEqual(filesByNotes[0].GetNotes(), new[] { notes[0] }));
            Assert.IsTrue(NoteEquality.AreEqual(filesByNotes[1].GetNotes(), new[] { notes[1] }));

            var timedEvents = midiFile.GetTimedEvents().Where(e => !(e.Event is NoteEvent)).ToList();

            Assert.IsTrue(TimedEventEquality.AreEqual(filesByNotes[0].GetTimedEvents()
                                                                     .Where(e => !(e.Event is NoteEvent)),
                                                      timedEvents,
                                                      false));
            Assert.IsTrue(TimedEventEquality.AreEqual(filesByNotes[1].GetTimedEvents()
                                                                     .Where(e => !(e.Event is NoteEvent)),
                                                      timedEvents,
                                                      false));
        }

        [Test]
        public void SplitByNotes_DifferentChannels()
        {
            var tempoMap = TempoMap.Default;
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)4;

            var trackChunk1 = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Quarter)
                .SetOctave(2)

                .Note(NoteName.A)
                .Note(NoteName.C)

                .Build()
                .ToTrackChunk(tempoMap, channel1);

            var trackChunk2 = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Eighth)
                .SetOctave(3)
                .StepForward(MusicalTimeSpan.ThirtySecond)

                .Note(NoteName.D)
                .Note(NoteName.DSharp)

                .Build()
                .ToTrackChunk(tempoMap, channel2);

            var midiFile = new MidiFile(trackChunk1, trackChunk2);
            var notes = midiFile.GetNotes().ToList();

            var filesByNotes = midiFile.SplitByNotes().ToList();
            Assert.AreEqual(4, filesByNotes.Count, "New files count is invalid.");

            Assert.IsTrue(NoteEquality.AreEqual(filesByNotes[0].GetNotes(), new[] { notes[0] }));
            Assert.IsTrue(NoteEquality.AreEqual(filesByNotes[1].GetNotes(), new[] { notes[1] }));
            Assert.IsTrue(NoteEquality.AreEqual(filesByNotes[2].GetNotes(), new[] { notes[2] }));
            Assert.IsTrue(NoteEquality.AreEqual(filesByNotes[3].GetNotes(), new[] { notes[3] }));
        }

        [Test]
        public void SplitByNotes_ValidFiles()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile = MidiFile.Read(filePath);

                var fileIndex = 0;
                var allNoteEventsCount = 0;
                var allNotesIds = new HashSet<NoteId>();

                foreach (var fileByNotes in midiFile.SplitByNotes())
                {
                    var noteEvents = fileByNotes.GetTrackChunks()
                                                .SelectMany(c => c.Events)
                                                .OfType<NoteEvent>()
                                                .ToList();
                    var notesIds = new HashSet<NoteId>(noteEvents.Select(n => n.GetNoteId()));

                    allNoteEventsCount += noteEvents.Count;
                    foreach (var noteId in notesIds)
                    {
                        allNotesIds.Add(noteId);
                    }

                    Assert.AreEqual(1,
                                    notesIds.Count,
                                    $"New file ({fileIndex}) contains different notes.");

                    fileIndex++;
                }

                var originalNoteEvents = midiFile.GetTrackChunks()
                                                 .SelectMany(c => c.Events)
                                                 .OfType<NoteEvent>()
                                                 .ToList();
                var originalNoteEventsCount = originalNoteEvents.Count();
                var originalNotesIds = new HashSet<NoteId>(originalNoteEvents.Select(e => e.GetNoteId()));

                Assert.AreEqual(originalNoteEventsCount,
                                allNoteEventsCount,
                                "Notes count of new files doesn't equal to count of notes of the original file.");

                Assert.IsTrue(originalNotesIds.SetEquals(allNotesIds),
                              "Notes in new files differ from notes in the original file.");
            }
        }

        #endregion

        #region SplitByGrid

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
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new SetTempoEvent(200000), 90),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 150),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 200)
                },
                "Second file contains invalid events.");

            CompareTimedEvents(
                newFiles[2].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new SetTempoEvent(200000), 90),
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

        #region SkipPart

        [Test]
        public void SkipPart_EmptyFile()
        {
            var midiFile = new MidiFile();
            var result = midiFile.SkipPart(MusicalTimeSpan.Eighth);

            Assert.IsTrue(result.IsEmpty(), "Empty file part skipping produced non-empty result.");
        }

        [Test]
        public void SkipPart_DontSplitNotes_DontPreserveTimes()
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

            var result = midiFile.SkipPart(partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(200000), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 50),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 100),
                    new TimedEvent(new TextEvent("Test"), 100)
                },
                "Resulting file contains invalid events.");
        }

        [Test]
        public void SkipPart_DontSplitNotes_PreserveTimes()
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

            var result = midiFile.SkipPart(partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new SetTempoEvent(200000), 90),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 150),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 200),
                    new TimedEvent(new PitchBendEvent(1000), 200),
                    new TimedEvent(new TextEvent("Test"), 200)
                },
                "Resulting file contains invalid events.");
        }

        [Test]
        public void SkipPart_SplitNotes()
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

            var result = midiFile.SkipPart(partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(100000), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 50),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 100)
                },
                "Resulting file contains invalid events.");
        }

        [Test]
        public void SkipPart_EmptyFiles()
        {
            var timedEvents = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 95)
            };

            var midiFile = timedEvents.ToFile();
            var partLength = (MidiTimeSpan)100;
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false
            };

            var result = midiFile.SkipPart(partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                Enumerable.Empty<TimedEvent>(),
                "Resulting file contains invalid events.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SkipPart_PreserveTrackChunks(bool preserveTrackChunks)
        {
            var timedEvents1 = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 95)
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

            var result = midiFile.SkipPart(partLength, settings);
            Assert.AreEqual(preserveTrackChunks ? 2 : 1, result.GetTrackChunks().Count(), "Track chunks count of resulting file is invalid.");

            if (preserveTrackChunks)
            {
                CompareTimedEvents(
                    result.GetTrackChunks().First().GetTimedEvents(),
                    Enumerable.Empty<TimedEvent>(),
                    "First track chunk of resulting file contains invalid events.");
            }

            CompareTimedEvents(
                result.GetTrackChunks().Last().GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)21, (SevenBitNumber)100), 0),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)21, (SevenBitNumber)100), 100)
                },
                "Second track chunk of resulting file contains invalid events.");
        }

        #endregion

        #region TakePart (from start)

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

        #region TakePart (in middle)

        [Test]
        public void TakePart_InMiddle_EmptyFile()
        {
            var midiFile = new MidiFile();
            var result = midiFile.TakePart(new MetricTimeSpan(0, 0, 1), MusicalTimeSpan.Eighth);

            Assert.IsTrue(result.IsEmpty(), "Empty file part taking produced non-empty result.");
        }

        [Test]
        public void TakePart_InMiddle_DontSplitNotes_DontPreserveTimes()
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
            var partStart = (MidiTimeSpan)100;
            var partLength = (MidiTimeSpan)100;
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false
            };

            var result = midiFile.TakePart(partStart, partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(200000), 0),
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 50),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 100),
                },
                "Resulting file contains invalid events.");
        }

        [Test]
        public void TakePart_InMiddle_DontSplitNotes_PreserveTimes()
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
            var partStart = (MidiTimeSpan)100;
            var partLength = (MidiTimeSpan)100;
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = true
            };

            var result = midiFile.TakePart(partStart, partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new InstrumentNameEvent("Test instrument"), 10),
                    new TimedEvent(new SetTempoEvent(200000), 90),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 150),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 200)
                },
                "Resulting file contains invalid events.");
        }

        [Test]
        public void TakePart_InMiddle_SplitNotes()
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
            var partStart = (MidiTimeSpan)100;
            var partLength = (MidiTimeSpan)100;
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = true,
                PreserveTimes = false
            };

            var result = midiFile.TakePart(partStart, partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new SetTempoEvent(100000), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 50),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 100)
                },
                "Resulting file contains invalid events.");
        }

        [Test]
        public void TakePart_InMiddle_EmptyFiles()
        {
            var timedEvents = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)100), 90),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70), 95),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)20, (SevenBitNumber)100), 300),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)20, (SevenBitNumber)100), 400)
            };

            var midiFile = timedEvents.ToFile();
            var partStart = (MidiTimeSpan)100;
            var partLength = (MidiTimeSpan)100;
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = false
            };

            var result = midiFile.TakePart(partStart, partLength, settings);

            CompareTimedEvents(
                result.GetTimedEvents(),
                Enumerable.Empty<TimedEvent>(),
                "Resulting file contains invalid events.");
        }

        #endregion

        #endregion

        #region Private methods

        private static void CompareTimedEvents(
            IEnumerable<TimedEvent> actualTimedEvents,
            IEnumerable<TimedEvent> expectedTimedEvents,
            string message)
        {
            Assert.IsTrue(TimedEventEquality.AreEqual(
                actualTimedEvents,
                expectedTimedEvents,
                false),
                message);
        }

        #endregion
    }
}
