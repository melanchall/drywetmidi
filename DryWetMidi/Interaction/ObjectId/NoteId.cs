using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class NoteId
    {
        #region Constructor

        public NoteId(FourBitNumber channel, SevenBitNumber noteNumber)
        {
            Channel = channel;
            NoteNumber = noteNumber;
        }

        #endregion

        #region Properties

        public FourBitNumber Channel { get; }

        public SevenBitNumber NoteNumber { get; }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var noteId = obj as NoteId;
            if (ReferenceEquals(noteId, null))
                return false;

            return Channel == noteId.Channel &&
                   NoteNumber == noteId.NoteNumber;
        }

        public override int GetHashCode()
        {
            return Channel * 1000 + NoteNumber;
        }

        #endregion
    }
}
