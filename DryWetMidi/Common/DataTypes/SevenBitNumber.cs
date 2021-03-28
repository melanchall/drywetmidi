using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// Type that is used to represent a seven-bit number (0-127; or in binary format 0000000-1111111).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Seven-bit numbers widely used by MIDI protocol as parameters of MIDI events (note number or
    /// velocity). Instead of manipulating built-in .NET numeric types (like <c>byte</c> or <c>int</c>)
    /// and checking for out-of-range errors all validation of numbers in the [0; 127] range happens
    /// on data type level via casting .NET integer values to the <see cref="SevenBitNumber"/>
    /// (see <see cref="op_Explicit(byte)"/>).
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// For example, to set a note's number:
    /// </para>
    /// <code language="csharp">
    /// var noteOnEvent = new NoteOnEvent();
    /// noteOnEvent.NoteNumber = (SevenBitNumber)100;
    /// </code>
    /// <para>
    /// or velocity:
    /// </para>
    /// <code>
    /// var noteOffEvent = new NoteOffEvent(SevenBitNumber.MinValue, (SevenBitNumber)70);
    /// </code>
    /// <para>
    /// where <c>SevenBitNumber.MinValue</c> passed to the <c>noteNumber</c> parameter and
    /// <c>(SevenBitNumber)70</c> passed to the <c>velocity</c> one.
    /// </para>
    /// </example>
    public struct SevenBitNumber : IComparable<SevenBitNumber>, IConvertible
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

        /// <summary>
        /// All possible values of <see cref="SevenBitNumber"/>.
        /// </summary>
        public static readonly SevenBitNumber[] Values = Enumerable.Range(MinValue, MaxValue - MinValue + 1)
                                                                   .Select(value => (SevenBitNumber)value)
                                                                   .ToArray();

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

        /// <summary>
        /// Converts the string representation of a seven-bit number to its <see cref="SevenBitNumber"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a number to convert.</param>
        /// <param name="sevenBitNumber">When this method returns, contains the <see cref="SevenBitNumber"/>
        /// equivalent of the seven-bit number contained in <paramref name="input"/>, if the conversion succeeded,
        /// or zero if the conversion failed. The conversion fails if the <paramref name="input"/> is <c>null</c> or
        /// <see cref="string.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, out SevenBitNumber sevenBitNumber)
        {
            sevenBitNumber = default(SevenBitNumber);

            byte byteValue;
            var parsed = ShortByteParser.TryParse(input, Min, Max, out byteValue).Status == ParsingStatus.Parsed;
            if (parsed)
                sevenBitNumber = (SevenBitNumber)byteValue;

            return parsed;
        }

        /// <summary>
        /// Converts the string representation of a seven-bit number to its <see cref="SevenBitNumber"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing a number to convert.</param>
        /// <returns>A <see cref="SevenBitNumber"/> equivalent to the seven-bit number contained in
        /// <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
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
        /// <returns><paramref name="number"/> represented as <see cref="byte"/>.</returns>
        public static implicit operator byte(SevenBitNumber number)
        {
            return number._value;
        }

        /// <summary>
        /// Converts the value of a <see cref="byte"/> to a <see cref="SevenBitNumber"/>.
        /// </summary>
        /// <param name="number">Byte value to convert to a <see cref="SevenBitNumber"/> object.</param>
        /// <returns><paramref name="number"/> represented as <see cref="SevenBitNumber"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is out of [0; 127] range.</exception>
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
        public int CompareTo(SevenBitNumber other)
        {
            return _value.CompareTo(other._value);
        }

        #endregion

        #region IConvertible

        /// <summary>
        /// Returns the <see cref="TypeCode"/> for value type <see cref="SevenBitNumber"/>.
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
