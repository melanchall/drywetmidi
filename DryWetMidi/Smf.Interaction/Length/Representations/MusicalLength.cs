using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents musical length of an object expressed in fraction of the whole note length.
    /// </summary>
    public sealed class MusicalLength : ILength
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicalLength"/> with the specified
        /// fraction of the whole note length.
        /// </summary>
        /// <param name="fraction">Fraction of the whole note length.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fraction"/> is null.</exception>
        public MusicalLength(Fraction fraction)
        {
            ThrowIfArgument.IsNull(nameof(fraction), fraction);

            Fraction = fraction;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the fraction component of the length represented by the current <see cref="MusicalLength"/>.
        /// </summary>
        public Fraction Fraction { get; }

        #endregion

        #region Methods

        public static bool TryParse(string input, out MusicalLength length)
        {
            return MusicalLengthParser.TryParse(input, out length).Status == ParsingStatus.Parsed;
        }

        public static MusicalLength Parse(string input)
        {
            var parsingResult = MusicalLengthParser.TryParse(input, out var length);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return length;

            throw parsingResult.Exception;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Casts <see cref="Common.Fraction"/> to <see cref="MusicalLength"/>.
        /// </summary>
        /// <param name="fraction"><see cref="Common.Fraction"/> to cast to <see cref="MusicalLength"/>.</param>
        public static implicit operator MusicalLength(Fraction fraction)
        {
            return new MusicalLength(fraction);
        }

        /// <summary>
        /// Casts <see cref="MusicalLength"/> to <see cref="Common.Fraction"/>.
        /// </summary>
        /// <param name="length"><see cref="MusicalLength"/> to cast to <see cref="Common.Fraction"/>.</param>
        public static implicit operator Fraction(MusicalLength length)
        {
            return length.Fraction;
        }

        /// <summary>
        /// Determines if two <see cref="MusicalLength"/> objects are equal.
        /// </summary>
        /// <param name="length1">The first <see cref="MusicalLength"/> to compare.</param>
        /// <param name="length2">The second <see cref="MusicalLength"/> to compare.</param>
        /// <returns>true if the lengths are equal, false otherwise.</returns>
        public static bool operator ==(MusicalLength length1, MusicalLength length2)
        {
            if (ReferenceEquals(length1, length2))
                return true;

            if (ReferenceEquals(null, length1) || ReferenceEquals(null, length2))
                return false;

            return length1.Fraction == length2.Fraction;
        }

        /// <summary>
        /// Determines if two <see cref="MusicalLength"/> objects are not equal.
        /// </summary>
        /// <param name="length1">The first <see cref="MusicalLength"/> to compare.</param>
        /// <param name="length2">The second <see cref="MusicalLength"/> to compare.</param>
        /// <returns>false if the lengths are equal, true otherwise.</returns>
        public static bool operator !=(MusicalLength length1, MusicalLength length2)
        {
            return !(length1 == length2);
        }

        /// <summary>
        /// Adds two specified <see cref="MusicalLength"/> objects.
        /// </summary>
        /// <param name="length1">The first <see cref="MusicalLength"/> to add.</param>
        /// <param name="length2">The second <see cref="MusicalLength"/> to add.</param>
        /// <returns>The sum of <paramref name="length1"/> and <paramref name="length2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="length1"/> is null. -or-
        /// <paramref name="length2"/> is null.</exception>
        public static MusicalLength operator +(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return new MusicalLength(length1.Fraction + length2.Fraction);
        }

        /// <summary>
        /// Subtracts one specified <see cref="MusicalLength"/> object from another.
        /// </summary>
        /// <param name="length1">The minuend.</param>
        /// <param name="length2">The subtrahend.</param>
        /// <returns>The result of subtracting <paramref name="length2"/> from <paramref name="length1"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="length1"/> is null. -or-
        /// <paramref name="length2"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="length1"/> is less than <paramref name="length2"/>.</exception>
        public static MusicalLength operator -(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            if (length1.Fraction < length2.Fraction)
                throw new ArgumentException("First length is less than second one.", nameof(length1));

            return new MusicalLength(length1.Fraction - length2.Fraction);
        }

        /// <summary>
        /// Determines if a <see cref="MusicalLength"/> is less than another one.
        /// </summary>
        /// <param name="length1">The first <see cref="MusicalLength"/>.</param>
        /// <param name="length2">The second <see cref="MusicalLength"/>.</param>
        /// <returns>true if the first length is less than the second, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="length1"/> is null. -or-
        /// <paramref name="length2"/> is null.</exception>
        public static bool operator <(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Fraction < length2.Fraction;
        }

        /// <summary>
        /// Determines if a <see cref="MusicalLength"/> is greater than another one.
        /// </summary>
        /// <param name="length1">The first <see cref="MusicalLength"/>.</param>
        /// <param name="length2">The second <see cref="MusicalLength"/>.</param>
        /// <returns>true if the first length is greater than the second, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="length1"/> is null. -or-
        /// <paramref name="length2"/> is null.</exception>
        public static bool operator >(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Fraction > length2.Fraction;
        }

        /// <summary>
        /// Determines if a <see cref="MusicalLength"/> is less than or equal to another one.
        /// </summary>
        /// <param name="length1">The first <see cref="MusicalLength"/>.</param>
        /// <param name="length2">The second <see cref="MusicalLength"/>.</param>
        /// <returns>true if the first length is less than or equal to the second, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="length1"/> is null. -or-
        /// <paramref name="length2"/> is null.</exception>
        public static bool operator <=(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Fraction <= length2.Fraction;
        }

        /// <summary>
        /// Determines if a <see cref="MusicalLength"/> is greater than or equal to another one.
        /// </summary>
        /// <param name="length1">The first <see cref="MusicalLength"/>.</param>
        /// <param name="length2">The second <see cref="MusicalLength"/>.</param>
        /// <returns>true if the first length is greater than or equal to the second, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="length1"/> is null. -or-
        /// <paramref name="length2"/> is null.</exception>
        public static bool operator >=(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Fraction >= length2.Fraction;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as MusicalLength);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Fraction.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Fraction.ToString();
        }

        #endregion

        #region ILength

        public ILength Add(ILength length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            var musicalLength = length as MusicalLength;
            return musicalLength != null
                ? this + musicalLength
                : LengthUtilities.Add(this, length);
        }

        public ILength Subtract(ILength length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            var musicalLength = length as MusicalLength;
            return musicalLength != null
                ? this - musicalLength
                : LengthUtilities.Subtract(this, length);
        }

        public ILength Multiply(int multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MusicalLength(Fraction * multiplier);
        }

        public ILength Divide(int divisor)
        {
            ThrowIfArgument.IsNegative(nameof(divisor), divisor, "Divisor is negative.");

            return new MusicalLength(Fraction / divisor);
        }

        #endregion
    }
}
