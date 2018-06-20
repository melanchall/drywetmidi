using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    // TODO: settings (velocity/off velocity combining, tolerance...)
    public static class NotesMerger
    {
        #region Methods

        public static IEnumerable<Note> Merge(this IEnumerable<Note> notes)
        {
            var currentNotes = new Dictionary<FourBitNumber, Dictionary<SevenBitNumber, Note>>();

            foreach (var note in notes.OrderBy(n => n.Time))
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
                }
                else
                {
                    yield return currentNotesByNoteNumber[note.NoteNumber];
                    currentNotesByNoteNumber[note.NoteNumber] = note;
                }
            }

            foreach (var note in currentNotes.SelectMany(kv => kv.Value.Values))
            {
                yield return note;
            }
        }

        #endregion
    }
}
