using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class ChordId : IObjectId
    {
        #region Constructor

        public ChordId(ICollection<NoteId> notesIds)
        {
            NotesIds = notesIds;
        }

        #endregion

        #region Properties

        public ICollection<NoteId> NotesIds { get; }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var chordId = obj as ChordId;
            if (ReferenceEquals(chordId, null))
                return false;

            // TODO: ignore order
            return NotesIds.SequenceEqual(chordId.NotesIds);
        }

        public override int GetHashCode()
        {
            return NotesIds.Sum(id => id.GetHashCode());
        }

        #endregion
    }
}
