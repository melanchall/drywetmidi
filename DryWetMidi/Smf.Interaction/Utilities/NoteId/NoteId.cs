using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
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
            if (ReferenceEquals(obj, null))
                return false;

            return Channel == noteId.Channel &&
                   NoteNumber == noteId.NoteNumber;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + Channel.GetHashCode();
                result = result * 23 + NoteNumber.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
