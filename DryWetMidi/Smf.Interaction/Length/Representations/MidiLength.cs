using Melanchall.DryWetMidi.Common;
using System;

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

        #region Methods

        public static bool TryParse(string input, out MidiLength length)
        {
            return MidiLengthParser.TryParse(input, out length).Status == ParsingStatus.Parsed;
        }

        public static MidiLength Parse(string input)
        {
            var parsingResult = MidiLengthParser.TryParse(input, out var length);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return length;

            throw parsingResult.Exception;
        }

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

        public static MidiLength operator +(MidiLength length1, MidiLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return new MidiLength(length1.Length + length2.Length);
        }

        public static MidiLength operator -(MidiLength length1, MidiLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            if (length1.Length < length2.Length)
                throw new ArgumentException("First length is less than second one.", nameof(length1));

            return new MidiLength(length1.Length - length2.Length);
        }

        public static bool operator <(MidiLength length1, MidiLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Length < length2.Length;
        }

        public static bool operator >(MidiLength length1, MidiLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Length > length2.Length;
        }

        public static bool operator <=(MidiLength length1, MidiLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Length <= length2.Length;
        }

        public static bool operator >=(MidiLength length1, MidiLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Length >= length2.Length;
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

        public ILength Add(ILength length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            var midiLength = length as MidiLength;
            return midiLength != null
                ? this + midiLength
                : LengthUtilities.Add(this, length);
        }

        public ILength Subtract(ILength length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            var midiLength = length as MidiLength;
            return midiLength != null
                ? this - midiLength
                : LengthUtilities.Subtract(this, length);
        }

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
