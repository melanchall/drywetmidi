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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var chordId = obj as ChordId;
            if (ReferenceEquals(chordId, null))
                return false;

            var length = NotesIds.Length;
            if (length != chordId.NotesIds.Length)
                return false;

            var set = new HashSet<NoteId>();

            for (var i = 0; i < length; i++)
            {
                var x = NotesIds[i];
                var y = chordId.NotesIds[i];

                if (!set.Add(x))
                    set.Remove(x);

                if (!set.Add(y))
                    set.Remove(y);
            }

            return !set.Any();
        }

        public override int GetHashCode()
        {
            return NotesIds.Sum(id => id.GetHashCode());
        }

        #endregion
    }
}
