using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MidiLength : ILength
    {
        #region Constructor

        public MidiLength(long length)
        {
            ThrowIfLengthArgument.IsNegative(nameof(length), length);

            Length = length;
        }

        #endregion

        #region Properties

        public long Length { get; }

        #endregion

        #region Operators

        public static explicit operator MidiLength(long length)
        {
            return new MidiLength(length);
        }

        public static implicit operator long(MidiLength length)
        {
            return length.Length;
        }

        public static bool operator ==(MidiLength length1, MidiLength length2)
        {
            if (ReferenceEquals(length1, length2))
                return true;

            if (ReferenceEquals(null, length1) || ReferenceEquals(null, length2))
                return false;

            return length1.Length == length2.Length;
        }

        public static bool operator !=(MidiLength length1, MidiLength length2)
        {
            return !(length1 == length2);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Length.ToString();
        }

        public override bool Equals(object obj)
        {
            return this == (obj as MidiLength);
        }

        public override int GetHashCode()
        {
            return Length.GetHashCode();
        }

        #endregion

        #region ILength

        public ILength Multiply(int multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MidiLength(Length * multiplier);
        }

        public ILength Divide(int divisor)
        {
            ThrowIfArgument.IsNegative(nameof(divisor), divisor, "Divisor is negative.");

            return new MidiLength(Length / divisor);
        }

        #endregion
    }
}
