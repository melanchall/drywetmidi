using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// Type that is used to represent a four-bit number (0-15; or in binary format 0000-1111).
    /// </summary>
    /// <remarks>
    /// Four-bit numbers widely used by MIDI protocol as parameters of MIDI events
    /// (channel number, for example). Instead of manipulating built-in .NET numeric types
    /// (like <c>byte</c> or <c>int</c>) and checking for out-of-range errors all validation of numbers
    /// in the [0; 15] range happens on data type level via casting .NET integer values to
    /// the <see cref="FourBitNumber"/> (see <see cref="op_Explicit(byte)"/>).
    /// </remarks>
    /// <example>
    /// <para>
    /// For example, to set a note's channel:
    /// </para>
    /// <code language="csharp">
    /// var noteOnEvent = new NoteOnEvent();
    /// noteOnEvent.Channel = (FourBitNumber)10;
    /// </code>
    /// </example>
    public struct FourBitNumber : IComparable<FourBitNumber>, IConvertible
    {
        #region Constants

        /// <summary>
        /// The smallest possible value of a <see cref="FourBitNumber"/>.
        /// </summary>
        public static readonly FourBitNumber MinValue = new FourBitNumber(Min);

        /// <summary>
        /// The largest possible value of a <see cref="FourBitNumber"/>.
        /// </summary>
        public static readonly FourBitNumber MaxValue = new FourBitNumber(Max);

        /// <summary>
        /// All possible values of <see cref="FourBitNumber"/>.
        /// </summary>
        public static readonly FourBitNumber[] Values = Enumerable.Range(MinValue, MaxValue - MinValue + 1)
                                                                  .Select(value => (FourBitNumber)value)
                                                                  .ToArray();

        private const byte Min = 0;
        private const byte Max = 15; // 00001111

        #endregion

        #region Fields

        private readonly byte _value;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FourBitNumber"/> with the specified value.
        /// </summary>
        /// <param name="value">Value representing four-bit number.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is out of
        /// [<see cref="MinValue"/>; <see cref="MaxValue"/>] range.</exception>
        public FourBitNumber(byte value)
        {
            ThrowIfArgument.IsOutOfRange(nameof(value), value, Min, Max, "Value is out of range valid for four-bit number.");

            _value = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the string representation of a four-bit number to its <see cref="FourBitNumber"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a number to convert.</param>
        /// <param name="fourBitNumber">When this method returns, contains the <see cref="FourBitNumber"/>
        /// equivalent of the four-bit number contained in <paramref name="input"/>, if the conversion succeeded,
        /// or zero if the conversion failed. The conversion fails if the <paramref name="input"/> is <c>null</c> or
        /// <see cref="string.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, out FourBitNumber fourBitNumber)
        {
            fourBitNumber = default(FourBitNumber);

            byte byteValue;
            var parsed = ShortByteParser.TryParse(input, Min, Max, out byteValue).Status == ParsingStatus.Parsed;
            if (parsed)
                fourBitNumber = (FourBitNumber)byteValue;

            return parsed;
        }

        /// <summary>
        /// Converts the string representation of a four-bit number to its <see cref="FourBitNumber"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing a number to convert.</param>
        /// <returns>A <see cref="FourBitNumber"/> equivalent to the four-bit number contained in
        /// <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static FourBitNumber Parse(string input)
        {
            byte byteValue;
            var parsingResult = ShortByteParser.TryParse(input, Min, Max, out byteValue);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return (FourBitNumber)byteValue;

            throw parsingResult.Exception;
        }

        #endregion

        #region Casting

        /// <summary>
        /// Converts the value of a <see cref="FourBitNumber"/> to a <see cref="byte"/>.
        /// </summary>
        /// <param name="number"><see cref="FourBitNumber"/> object to convert to a byte value.</param>
        /// <returns><paramref name="number"/> represented as <see cref="byte"/>.</returns>
        public static implicit operator byte(FourBitNumber number)
        {
            return number._value;
        }

        /// <summary>
        /// Converts the value of a <see cref="byte"/> to a <see cref="FourBitNumber"/>.
        /// </summary>
        /// <param name="number">Byte value to convert to a <see cref="FourBitNumber"/> object.</param>
        /// <returns><paramref name="number"/> represented as <see cref="FourBitNumber"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is out of [0; 15] range.</exception>
        public static explicit operator FourBitNumber(byte number)
        {
            return new FourBitNumber(number);
        }

        #endregion

        #region IComparable<FourBitNumber>

        /// <summary>
        /// Compares the current instance with another object of the same type and returns
        /// an integer that indicates whether the current instance precedes, follows, or
        /// occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns><para>A value that indicates the relative order of the objects being compared. The
        /// return value has these meanings:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Value</term>
        /// <description>Meaning</description>
        /// </listheader>
        /// <item>
        /// <term>Less than zero</term>
        /// <description>This instance precedes <paramref name="other"/> in the sort order.</description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>This instance occurs in the same position in the sort order as <paramref name="other"/>.</description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>This instance follows <paramref name="other"/> in the sort order.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(FourBitNumber other)
        {
            return _value.CompareTo(other._value);
        }

        #endregion

        #region IConvertible

        /// <summary>
        /// Returns the <see cref="TypeCode"/> for value type <see cref="FourBitNumber"/>.
        /// </summary>
        /// <returns>The enumerated constant, <see cref="TypeCode.Byte"/>.</returns>
        public TypeCode GetTypeCode()
        {
            return _value.GetTypeCode();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="bool"/> value using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="bool"/> value equivalent to the value of this instance.</returns>
        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToBoolean(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent Unicode character using
        /// the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>A Unicode character equivalent to the value of this instance.</returns>
        char IConvertible.ToChar(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToChar(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit signed integer using
        /// the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>An 8-bit signed integer equivalent to the value of this instance.</returns>
        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToSByte(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 8-bit unsigned integer using
        /// the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>An 8-bit unsigned integer equivalent to the value of this instance.</returns>
        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToByte(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit signed integer using
        /// the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>An 16-bit signed integer equivalent to the value of this instance.</returns>
        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToInt16(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 16-bit unsigned integer
        /// using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>An 16-bit unsigned integer equivalent to the value of this instance.</returns>
        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToUInt16(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit signed integer using
        /// the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>An 32-bit signed integer equivalent to the value of this instance.</returns>
        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToInt32(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 32-bit unsigned integer
        /// using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>An 32-bit unsigned integer equivalent to the value of this instance.</returns>
        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToUInt32(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit signed integer using
        /// the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>An 64-bit signed integer equivalent to the value of this instance.</returns>
        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToInt64(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent 64-bit unsigned integer
        /// using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>An 64-bit unsigned integer equivalent to the value of this instance.</returns>
        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToUInt64(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent single-precision floating-point
        /// number using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>A single-precision floating-point number equivalent to the value of this instance.</returns>
        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToSingle(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent double-precision floating-point
        /// number using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>A double-precision floating-point number equivalent to the value of this instance.</returns>
        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToDouble(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="decimal"/> number using
        /// the specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="decimal"/> number equivalent to the value of this instance.</returns>
        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToDecimal(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="DateTime"/> using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="DateTime"/> instance equivalent to the value of this instance.</returns>
        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)_value).ToDateTime(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="string"/> using the
        /// specified culture-specific formatting information.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="string"/> instance equivalent to the value of this instance.</returns>
        string IConvertible.ToString(IFormatProvider provider)
        {
            return _value.ToString(provider);
        }

        /// <summary>
        /// Converts the value of this instance to an System.Object of the specified <see cref="Type"/>
        /// that has an equivalent value, using the specified culture-specific formatting
        /// information.
        /// </summary>
        /// <param name="conversionType">The <see cref="Type"/> to which the value of this instance is converted.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that
        /// supplies culture-specific formatting information.</param>
        /// <returns>An <see cref="object"/> instance of type conversionType whose value is equivalent to
        /// the value of this instance.</returns>
        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)_value).ToType(conversionType, provider);
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
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is FourBitNumber))
                return false;

            var fourBitNumber = (FourBitNumber)obj;
            return fourBitNumber._value == _value;
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
