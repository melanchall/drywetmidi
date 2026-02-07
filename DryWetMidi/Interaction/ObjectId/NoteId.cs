namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class NoteId
    {
        #region Fields

        private readonly int _id;

        #endregion

        #region Constructor

        public NoteId(int id)
        {
            _id = id;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var noteId = obj as NoteId;
            if (ReferenceEquals(noteId, null))
                return false;

            return _id == noteId._id;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        #endregion
    }
}
