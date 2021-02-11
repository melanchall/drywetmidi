using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class MidiFileSplitterTests
    {
        #region Test methods

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
                .SetOctave(Octave.Get(2))

                .Note(NoteName.A)

                .ProgramChange((SevenBitNumber)20)
                .Note(NoteName.C)

                .Build()
                .ToFile(tempoMap);

            var filesByNotes = midiFile.SplitByNotes().ToList();
            Assert.AreEqual(2, filesByNotes.Count, "New files count is invalid.");

            var notes = midiFile.GetNotes().ToList();

            MidiAsserts.AreEqual(new[] { notes[0] }, filesByNotes[0].GetNotes(), "Notes of first file are invalid.");
            MidiAsserts.AreEqual(new[] { notes[1] }, filesByNotes[1].GetNotes(), "Notes of second file are invalid.");

            var timedEvents = midiFile.GetTimedEvents().Where(e => !(e.Event is NoteEvent)).ToList();

            MidiAsserts.AreEqual(
                filesByNotes[0].GetTimedEvents().Where(e => !(e.Event is NoteEvent)),
                timedEvents,
                false,
                0,
                "Events 0 are invalid.");
            MidiAsserts.AreEqual(
                filesByNotes[1].GetTimedEvents().Where(e => !(e.Event is NoteEvent)),
                timedEvents,
                false,
                0,
                "Events 1 are invalid.");
        }

        [Test]
        public void SplitByNotes_DifferentChannels()
        {
            var tempoMap = TempoMap.Default;
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)4;

            var trackChunk1 = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Quarter)
                .SetOctave(Octave.Get(2))

                .Note(NoteName.A)
                .Note(NoteName.C)

                .Build()
                .ToTrackChunk(tempoMap, channel1);

            var trackChunk2 = new PatternBuilder()
                .SetNoteLength(MusicalTimeSpan.Eighth)
                .SetOctave(Octave.Get(3))
                .StepForward(MusicalTimeSpan.ThirtySecond)

                .Note(NoteName.D)
                .Note(NoteName.DSharp)

                .Build()
                .ToTrackChunk(tempoMap, channel2);

            var midiFile = new MidiFile(trackChunk1, trackChunk2);
            var notes = midiFile.GetNotes().ToList();

            var filesByNotes = midiFile.SplitByNotes().ToList();
            Assert.AreEqual(4, filesByNotes.Count, "New files count is invalid.");

            MidiAsserts.AreEqual(new[] { notes[0] }, filesByNotes[0].GetNotes(), "Notes 0 are invalid.");
            MidiAsserts.AreEqual(new[] { notes[1] }, filesByNotes[1].GetNotes(), "Notes 1 are invalid.");
            MidiAsserts.AreEqual(new[] { notes[2] }, filesByNotes[2].GetNotes(), "Notes 2 are invalid.");
            MidiAsserts.AreEqual(new[] { notes[3] }, filesByNotes[3].GetNotes(), "Notes 3 are invalid.");
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
    }
}
