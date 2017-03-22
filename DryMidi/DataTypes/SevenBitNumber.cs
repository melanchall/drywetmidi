using System;

namespace Melanchall.DryMidi
{
    /// <summary>
    /// Type that is used to represent a seven-bit number.
    /// </summary>
    /// <remarks>Seven-bit numbers widely used by MIDI protocol as parameters of MIDI-messages.
    /// So instead of manipulating built-in C# numeric types (like byte or int) and checking for
    /// out-of-range errors all validation of numbers in the [0; 127] range happens on data type
    /// level via casting C# integer values to <see cref="SevenBitNumber"/> type.</remarks>
    public struct SevenBitNumber
    {
        #region Constants

        /// <summary>
        /// The smallest possible value of <see cref="SevenBitNumber"/>.
        /// </summary>
        public static readonly SevenBitNumber MinValue = new SevenBitNumber(Min);

        /// <summary>
        /// The largest possible value of an <see cref="SevenBitNumber"/>.
        /// </summary>
        public static readonly SevenBitNumber MaxValue = new SevenBitNumber(Max);

        private const byte Min = 0;
        private const byte Max = 127; // 01111111

        #endregion

        #region Fields

        private byte _value;

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
            if (value < Min || value > Max)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value is out of range valid for seven-bit number.");

            _value = value;
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

        #region Overrides

        /// <summary>
        /// Converts the current <see cref="SevenBitNumber"/> object to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the value of this instance.</returns>
        public override string ToString()
        {
            return _value.ToString();
        }

        #endregion
    }
}
