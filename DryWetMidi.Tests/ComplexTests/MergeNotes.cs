using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.ComplexTests
{
    [TestClass]
    public class MergeNotes
    {
        #region Test methods

        [TestMethod]
        [Description("All notes have the same channel should be merged into one.")]
        public void Merge_SingleChannel_MergeAll()
        {
            var midiFile = new MidiFile(new TrackChunk());

            using (var notesManager = midiFile.GetTrackChunks().First().ManageNotes())
            {
                var notes = notesManager.Notes;
                notes.Add(new Note(SevenBitNumber.MaxValue, 10),
                          new Note(SevenBitNumber.MaxValue, 10, 2),
                          new Note(SevenBitNumber.MaxValue, 10, 9),
                          new Note(SevenBitNumber.MaxValue, 10, 1),
                          new Note(SevenBitNumber.MaxValue, 10, 16),
                          new Note(SevenBitNumber.MaxValue, 10, 25));
            }

            Merge(midiFile);

            var actualNotes = midiFile.GetNotes().ToList();

            Assert.AreEqual(1, actualNotes.Count);
            Assert.AreEqual(35, actualNotes.First().Length);
        }

        #endregion

        #region Private methods

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
                var currentNotes = new Dictionary<FourBitNumber, Note>();

                foreach (var note in notes.ToList())
                {
                    var channel = note.Channel;

                    Note currentNote;
                    if (!currentNotes.TryGetValue(channel, out currentNote))
                    {
                        currentNotes.Add(channel, currentNote = note);
                        continue;
                    }

                    var currentEndTime = currentNote.Time + currentNote.Length;
                    if (note.Time < currentEndTime)
                    {
                        var endTime = Math.Max(note.Time + note.Length, currentEndTime);
                        currentNote.Length = endTime - currentNote.Time;

                        notes.Remove(note);
                    }
                }
            }
        }

        #endregion
    }
}
