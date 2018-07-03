using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class Rest : ILengthedObject
    {
        #region Constructor

        internal Rest(long time, long length, FourBitNumber? channel, SevenBitNumber? noteNumber)
        {
            Time = time;
            Length = length;
            Channel = channel;
            NoteNumber = noteNumber;
        }

        #endregion

        #region Properties

        public long Time { get; }

        public long Length { get; }

        public FourBitNumber? Channel { get; }

        public SevenBitNumber? NoteNumber { get; }

        #endregion

        #region Operators

        public static bool operator ==(Rest rest1, Rest rest2)
        {
            if (ReferenceEquals(rest1, rest2))
                return true;

            if (ReferenceEquals(null, rest1) || ReferenceEquals(null, rest2))
                return false;

            return rest1.Time == rest2.Time &&
                   rest1.Length == rest2.Length &&
                   rest1.Channel == rest2.Channel &&
                   rest1.NoteNumber == rest2.NoteNumber;
        }

        public static bool operator !=(Rest rest1, Rest rest2)
        {
            return !(rest1 == rest2);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Rest (channel = {Channel}, note number = {NoteNumber})";
        }

        public override bool Equals(object obj)
        {
            return this == (obj as Rest);
        }

        public override int GetHashCode()
        {
            return Time.GetHashCode() ^
                   Length.GetHashCode() ^
                   Channel.GetValueOrDefault().GetHashCode() ^
                   NoteNumber.GetValueOrDefault().GetHashCode();
        }

        #endregion
    }
}
