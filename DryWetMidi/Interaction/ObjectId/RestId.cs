using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class RestId : IObjectId
    {
        #region Constructor

        public RestId(FourBitNumber? channel, SevenBitNumber? noteNumber)
        {
            Channel = channel;
            NoteNumber = noteNumber;
        }

        #endregion

        #region Properties

        public FourBitNumber? Channel { get; }

        public SevenBitNumber? NoteNumber { get; }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var restId = obj as RestId;
            if (ReferenceEquals(restId, null))
                return false;

            return Channel == restId.Channel &&
                   NoteNumber == restId.NoteNumber;
        }

        public override int GetHashCode()
        {
            return (Channel ?? 20) * 1000 + (NoteNumber ?? 200);
        }

        #endregion
    }
}
