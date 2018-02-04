using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests
{
    [TestClass]
    public class MergeNotes
    {
        #region Nested classes

        private struct NoteDescriptor
        {
            public NoteDescriptor(long startTime, long endTime, FourBitNumber channel, SevenBitNumber noteNumber)
            {
                StartTime = startTime;
                EndTime = endTime;
                Channel = channel;
                NoteNumber = noteNumber;
            }

            public long StartTime { get; }

            public long EndTime { get; }

            public FourBitNumber Channel { get; }

            public SevenBitNumber NoteNumber { get; }
        }

        #endregion

        #region Test methods

        [TestMethod]
        [Description("All notes have the same channel and note number, and overlap each other, thus should be merged into one.")]
        public void Merge_SingleChannel_SingleNoteNumber_AllOverlapped()
        {
            var noteNumber = SevenBitNumber.MaxValue;

            TestMerge(
                new[]
                {
                    new Note(noteNumber, 10),
                    new Note(noteNumber, 10, 2),
                    new Note(noteNumber, 10, 9),
                    new Note(noteNumber, 10, 1),
                    new Note(noteNumber, 10, 16),
                    new Note(noteNumber, 10, 25)
                },
                new[]
                {
                    new NoteDescriptor(0, 35, FourBitNumber.MinValue, noteNumber)
                });
        }

        [TestMethod]
        [Description("One note is placed inside another and thus greater note should be the result of merge.")]
        public void Merge_SingleChannel_SingleNoteNumber_NoteInsideAnother()
        {
            var noteNumber = SevenBitNumber.MaxValue;

            TestMerge(
                new[]
                {
                    new Note(noteNumber, 100, 10),
                    new Note(noteNumber, 40, 20)
                },
                new[]
                {
                    new NoteDescriptor(10, 110, FourBitNumber.MinValue, noteNumber)
                });
        }

        [TestMethod]
        [Description("All notes have the same channel and note number, some of them overlap each other, some not.")]
        public void Merge_SingleChannel_SingleNoteNumber_SeparatedAndOverlapped()
        {
            var channel = FourBitNumber.MinValue;
            var noteNumber = SevenBitNumber.MaxValue;

            TestMerge(
                new[]
                {
                    new Note(noteNumber, 10),
                    new Note(noteNumber, 10, 2),
                    new Note(noteNumber, 10, 9),
                    new Note(noteNumber, 10, 50),
                    new Note(noteNumber, 10, 70),
                    new Note(noteNumber, 10, 75)
                },
                new[]
                {
                    new NoteDescriptor(0, 19, channel, noteNumber),
                    new NoteDescriptor(50, 60, channel, noteNumber),
                    new NoteDescriptor(70, 85, channel, noteNumber)
                });
        }

        #endregion

        #region Private methods

        private static void TestMerge(IEnumerable<Note> inputNotes, params NoteDescriptor[] noteDescriptors)
        {
            var midiFile = GetMergedMidiFile(inputNotes);

            CollectionAssert.AreEqual(midiFile.GetNotes()
                                              .Select(n => new NoteDescriptor(n.Time,
                                                                              n.Time + n.Length,
                                                                              n.Channel,
                                                                              n.NoteNumber))
                                              .ToList(),
                                      noteDescriptors);
        }

        private static MidiFile GetMergedMidiFile(IEnumerable<Note> notes)
        {
            var midiFile = new MidiFile(new TrackChunk());

            using (var notesManager = midiFile.GetTrackChunks().First().ManageNotes())
            {
                notesManager.Notes.Add(notes);
            }

            Merge(midiFile);

            return midiFile;
        }

        private static void Merge(MidiFile midiFile)
        {
            foreach (var trackChunk in midiFile.GetTrackChunks())
            {
                Merge(trackChunk);
            }
        }

        private static void Merge(TrackChunk trackChunk)
        {
            using (var notesManager = trackChunk.ManageNotes())
            {
                var notes = notesManager.Notes;
                var currentNotes = new Dictionary<FourBitNumber, Dictionary<SevenBitNumber, Note>>();

                foreach (var note in notes.ToList())
                {
                    var channel = note.Channel;

                    Dictionary<SevenBitNumber, Note> currentNotesByNoteNumber;
                    if (!currentNotes.TryGetValue(channel, out currentNotesByNoteNumber))
                        currentNotes.Add(channel, currentNotesByNoteNumber = new Dictionary<SevenBitNumber, Note>());

                    Note currentNote;
                    if (!currentNotesByNoteNumber.TryGetValue(note.NoteNumber, out currentNote))
                    {
                        currentNotesByNoteNumber.Add(note.NoteNumber, currentNote = note);
                        continue;
                    }

                    var currentEndTime = currentNote.Time + currentNote.Length;
                    if (note.Time <= currentEndTime)
                    {
                        var endTime = Math.Max(note.Time + note.Length, currentEndTime);
                        currentNote.Length = endTime - currentNote.Time;

                        notes.Remove(note);
                    }
                    else
                        currentNotesByNoteNumber[note.NoteNumber] = note;
                }
            }
        }

        #endregion
    }
}
