using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
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
        [Description("Split valid MIDI files by channel.")]
        public void SplitByChannel_ValidFiles()
        {
            foreach (var filePath in TestFilesProvider.GetValidFiles())
            {
                var midiFile = MidiFile.Read(filePath);
                var originalChannels = midiFile.GetTrackChunks()
                                               .SelectMany(c => c.Events)
                                               .OfType<ChannelEvent>()
                                               .Select(e => e.Channel)
                                               .Distinct()
                                               .ToArray();

                var filesByChannel = midiFile.SplitByChannel().ToList();
                var allChannels = new List<FourBitNumber>(FourBitNumber.MaxValue + 1);

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
        [Description("Split empty MIDI file without track chunks by notes.")]
        public void SplitByNotes_EmptyFile()
        {
            var midiFile = new MidiFile();

            Assert.IsTrue(!midiFile.SplitByNotes().Any(), "Empty file splitting produced non-empty result.");
        }

        [Test]
        [Description("Split MIDI file without note events by notes.")]
        public void SplitByNotes_NoNoteEvents()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new SetTempoEvent(100000),
                    new TextEvent()));

            Assert.IsTrue(!midiFile.SplitByNotes().Any(), "Empty file splitting produced non-empty result.");
        }

        [Test]
        [Description("Split MIDI file with single channel notes by notes.")]
        public void SplitByNotes_SingleChannel()
        {
            var tempoMap = TempoMap.Default;

            var midiFile = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Quarter)
                .SetOctave(2)

                .Note(NoteName.A)
                .Note(NoteName.C)

                .Build()
                .ToFile(tempoMap);

            var notes = midiFile.GetNotes().ToList();

            var filesByNotes = midiFile.SplitByNotes().ToList();
            Assert.AreEqual(2, filesByNotes.Count, "New files count is invalid.");

            Assert.IsTrue(NoteEquality.AreEqual(filesByNotes[0].GetNotes(), new[] { notes[0] }));
            Assert.IsTrue(NoteEquality.AreEqual(filesByNotes[1].GetNotes(), new[] { notes[1] }));
        }

        [Test]
        [Description("Split MIDI file with notes of different channels by notes.")]
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
                .ToTrackChunk(tempoMap, channel1);

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
        [Description("Split valid MIDI files by notes.")]
        public void SplitByNotes_ValidFiles()
        {
            foreach (var filePath in TestFilesProvider.GetValidFiles())
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
                    var notesIds = new HashSet<NoteId>(noteEvents.Select(n => n.GetId()));

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
                var originalNotesIds = new HashSet<NoteId>(originalNoteEvents.Select(e => e.GetId()));

                Assert.AreEqual(originalNoteEventsCount,
                                allNoteEventsCount,
                                "Notes count of new files doesn't equal to count of notes of the original file.");

                Assert.IsTrue(originalNotesIds.SetEquals(allNotesIds),
                              "Notes in new files differ from notes in the original file.");
            }
        }

        #endregion

        #endregion
    }
}
