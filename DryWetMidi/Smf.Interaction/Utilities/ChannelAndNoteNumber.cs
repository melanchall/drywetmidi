using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class ChannelAndNoteNumber
    {
        #region Constructor

        public ChannelAndNoteNumber(FourBitNumber channel, SevenBitNumber noteNumber)
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

            var channelAndNoteNumber = obj as ChannelAndNoteNumber;
            if (ReferenceEquals(obj, null))
                return false;

            return Channel == channelAndNoteNumber.Channel &&
                   NoteNumber == channelAndNoteNumber.NoteNumber;
        }

        public override int GetHashCode()
        {
            return Channel.GetHashCode() ^ NoteNumber.GetHashCode();
        }

        #endregion
    }
}
