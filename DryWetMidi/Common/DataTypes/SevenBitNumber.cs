using System;

namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// Type that is used to represent a seven-bit number (0-127).
    /// </summary>
    /// <remarks>
    /// Seven-bit numbers widely used by MIDI protocol as parameters of MIDI events.
    /// So instead of manipulating built-in C# numeric types (like byte or int) and checking for
    /// out-of-range errors all validation of numbers in the [0; 127] range happens on data type
    /// level via casting C# integer values to the <see cref="SevenBitNumber"/>.
    /// </remarks>
    public struct SevenBitNumber : IComparable<SevenBitNumber>
    {
        #region Constants

        /// <summary>
        /// The smallest possible value of a <see cref="SevenBitNumber"/>.
        /// </summary>
        public static readonly SevenBitNumber MinValue = new SevenBitNumber(Min);

        /// <summary>
        /// The largest possible value of a <see cref="SevenBitNumber"/>.
        /// </summary>
        public static readonly SevenBitNumber MaxValue = new SevenBitNumber(Max);

        private const byte Min = 0;
        private const byte Max = 127; // 01111111

        #endregion

        #region Fields

        private readonly byte _value;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SevenBitNumber"/> with the specified value.
        /// </summary>
        /// <param name="value">Value representing seven-bit number.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is out of
        /// [<see cref="MinValue"/>; <see cref="MaxValue"/>] range.</exception>
        public SevenBitNumber(byte value)
        {
            ThrowIfArgument.IsOutOfRange(nameof(value), value, Min, Max, "Value is out of range valid for seven-bit number.");

            _value = value;
        }

        #endregion

        #region Methods

        public static bool TryParse(string input, out SevenBitNumber result)
        {
            result = default(SevenBitNumber);

            byte byteValue;
            var parsed = ShortByteParser.TryParse(input, Min, Max, out byteValue).Status == ParsingStatus.Parsed;
            if (parsed)
                result = (SevenBitNumber)byteValue;

            return parsed;
        }

        public static SevenBitNumber Parse(string input)
        {
            byte byteValue;
            var parsingResult = ShortByteParser.TryParse(input, Min, Max, out byteValue);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return (SevenBitNumber)byteValue;

            throw parsingResult.Exception;
        }

        #endregion

        #region Casting

        /// <summary>
        /// Converts the value of a <see cref="SevenBitNumber"/> to a <see cref="byte"/>.
        /// </summary>
        /// <param name="number"><see cref="SevenBitNumber"/> object to convert to a byte value.</param>
        public static implicit operator byte(SevenBitNumber number)
        {
            return number._value;
        }

        /// <summary>
        /// Converts the value of a <see cref="byte"/> to a <see cref="SevenBitNumber"/>.
        /// </summary>
        /// <param name="number">Byte value to convert to a <see cref="SevenBitNumber"/> object.</param>
        public static explicit operator SevenBitNumber(byte number)
        {
            return new SevenBitNumber(number);
        }

        #endregion

        #region IComparable<SevenBitNumber>

        /// <summary>
        /// Compares the current instance with another object of the same type and returns
        /// an integer that indicates whether the current instance precedes, follows, or
        /// occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The
        /// return value has these meanings:
        /// - Less than zero: This instance precedes other in the sort order.
        /// - Zero: This instance occurs in the same position in the sort order as other.
        /// - Greater than zero: This instance follows other in the sort order.</returns>
        public int CompareTo(SevenBitNumber other)
        {
            return _value.CompareTo(other._value);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return _value.ToString();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SevenBitNumber))
                return false;

            var sevenBitNumber = (SevenBitNumber)obj;
            return sevenBitNumber._value == _value;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion
    }
}
