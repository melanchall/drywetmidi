using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class ChordId
    {
        #region Constructor

        public ChordId(NoteId[] notesIds)
        {
            NotesIds = notesIds;
        }

        #endregion

        #region Properties

        public NoteId[] NotesIds { get; }

        #endregion

        #region Overrides

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var chordId = obj as ChordId;
            if (ReferenceEquals(chordId, null))
                return false;

            var length = NotesIds.Length;
            if (length != chordId.NotesIds.Length)
                return false;

            var notesIdsCounts = new Dictionary<NoteId, int>();

            for (var i = 0; i < length; i++)
            {
                var noteId = NotesIds[i];
                notesIdsCounts.TryGetValue(noteId, out var count);
                notesIdsCounts[noteId] = count + 1;
            }

            for (var i = 0; i < length; i++)
            {
                var noteId = chordId.NotesIds[i];
                if (!notesIdsCounts.TryGetValue(noteId, out var count))
                    return false;

                if (count == 1)
                    notesIdsCounts.Remove(noteId);
                else
                    notesIdsCounts[noteId] = count - 1;
            }

            return notesIdsCounts.Count == 0;
        }

        public override int GetHashCode()
        {
            return NotesIds.Sum(id => id.GetHashCode());
        }

        #endregion
    }
}
