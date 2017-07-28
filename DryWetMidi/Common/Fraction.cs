using System;

namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// Represents a mathematical simple fraction defined by nonnegative numerator
    /// and positive denominator.
    /// </summary>
    public sealed class Fraction
    {
        #region Constants

        /// <summary>
        /// The fraction with numerator of 0 and denominator of 1.
        /// </summary>
        public static readonly Fraction ZeroFraction = new Fraction();

        private const long DefaultNumerator = 0;
        private const long DefaultDenominator = 1;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Fraction"/>.
        /// </summary>
        public Fraction()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fraction"/> with the specified
        /// numerator and denominator.
        /// </summary>
        /// <param name="numerator">The numerator of a fraction.</param>
        /// <param name="denominator">The denominator of a fraction.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="numerator"/> is negative. -or-
        /// <paramref name="denominator"/> is zero or negative.</exception>
        public Fraction(long numerator, long denominator)
        {
            ThrowIfArgument.IsNegative(nameof(numerator), numerator, "Numerator is negative.");
            ThrowIfArgument.IsNonpositive(nameof(denominator), denominator, "Denominator is zero or negative.");

            // Simplify a fraction

            var greatestCommonDivisor = MathUtilities.GreatestCommonDivisor(numerator, denominator);
            numerator /= greatestCommonDivisor;
            denominator /= greatestCommonDivisor;

            Numerator = numerator;
            Denominator = denominator;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the numerator of the current <see cref="Fraction"/>.
        /// </summary>
        public long Numerator { get; } = DefaultNumerator;

        /// <summary>
        /// Gets the denominator of the current <see cref="Fraction"/>.
        /// </summary>
        public long Denominator { get; } = DefaultDenominator;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="fraction">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(Fraction fraction)
        {
            return this == fraction;
        }

        /// <summary>
        /// Reduces the specified fractions to the common denominator.
        /// </summary>
        /// <param name="fraction1">First fraction.</param>
        /// <param name="fraction2">Second fraction.</param>
        /// <param name="numerator1">Numerator of the reduced first fraction.</param>
        /// <param name="numerator2">Numerator of the reduced second fraction.</param>
        /// <param name="denominator">Common denominator of reduced fractions.</param>
        private static void ReduceToCommonDenominator(Fraction fraction1, Fraction fraction2, out long numerator1, out long numerator2, out long denominator)
        {
            denominator = MathUtilities.LeastCommonMultiple(fraction1.Denominator, fraction2.Denominator);

            numerator1 = fraction1.Numerator * denominator / fraction1.Denominator;
            numerator2 = fraction2.Numerator * denominator / fraction2.Denominator;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="Fraction"/> objects are equal.
        /// </summary>
        /// <param name="fraction1">The first fraction to compare.</param>
        /// <param name="fraction2">The second fraction to compare.</param>
        /// <returns>true if the fractions are equal, false otherwise.</returns>
        public static bool operator ==(Fraction fraction1, Fraction fraction2)
        {
            if (ReferenceEquals(fraction1, fraction2))
                return true;

            if (ReferenceEquals(null, fraction1) || ReferenceEquals(null, fraction2))
                return false;

            ReduceToCommonDenominator(fraction1, fraction2, out var numerator1, out var numerator2, out _);
            return numerator1 == numerator2;
        }

        /// <summary>
        /// Determines if two <see cref="Fraction"/> objects are not equal.
        /// </summary>
        /// <param name="fraction1">The first fraction to compare.</param>
        /// <param name="fraction2">The second fraction to compare.</param>
        /// <returns>false if the fractions are equal, true otherwise.</returns>
        public static bool operator !=(Fraction fraction1, Fraction fraction2)
        {
            return !(fraction1 == fraction2);
        }

        /// <summary>
        /// Multiplies the specified <see cref="Fraction"/> by nonnegative integer number.
        /// </summary>
        /// <param name="fraction">The multiplicand.</param>
        /// <param name="number">The multiplier.</param>
        /// <returns>The product of <paramref name="fraction"/> and <paramref name="number"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fraction"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is negative.</exception>
        public static Fraction operator *(Fraction fraction, long number)
        {
            ThrowIfArgument.IsNull(nameof(fraction), fraction);
            ThrowIfArgument.IsNegative(nameof(number), number, "Number is negative.");

            return new Fraction(fraction.Numerator * number,
                                fraction.Denominator);
        }

        /// <summary>
        /// Multiplies the specified nonnegative integer number by <see cref="Fraction"/>.
        /// </summary>
        /// <param name="number">The multiplicand.</param>
        /// <param name="fraction">The multiplier.</param>
        /// <returns>The product of <paramref name="number"/> and <paramref name="fraction"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fraction"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is negative.</exception>
        public static Fraction operator *(long number, Fraction fraction)
        {
            return fraction * number;
        }

        /// <summary>
        /// Multiplies two specified <see cref="Fraction"/> objects.
        /// </summary>
        /// <param name="fraction1">The multiplicand.</param>
        /// <param name="fraction2">The multiplier.</param>
        /// <returns>The product of <paramref name="fraction1"/> and <paramref name="fraction2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fraction1"/> is null. -or-
        /// <paramref name="fraction2"/> is null.</exception>
        public static Fraction operator *(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            return new Fraction(fraction1.Numerator * fraction2.Numerator,
                                fraction1.Denominator * fraction2.Denominator);
        }

        /// <summary>
        /// Divides the specified <see cref="Fraction"/> by positive integer number. 
        /// </summary>
        /// <param name="fraction">The dividend.</param>
        /// <param name="number">The divisor.</param>
        /// <returns>Quotient of <paramref name="fraction"/> and <paramref name="number"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fraction"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is zero or negative.</exception>
        public static Fraction operator /(Fraction fraction, long number)
        {
            ThrowIfArgument.IsNull(nameof(fraction), fraction);
            ThrowIfArgument.IsNonpositive(nameof(number), number, "Number is zero or negative.");

            return new Fraction(fraction.Numerator,
                                fraction.Denominator * number);
        }

        /// <summary>
        /// Divides two specified <see cref="Fraction"/> objects.
        /// </summary>
        /// <param name="fraction1">The dividend.</param>
        /// <param name="fraction2">The divisor.</param>
        /// <returns>Quotient of <paramref name="fraction1"/> and <paramref name="fraction2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fraction1"/> is null. -or-
        /// <paramref name="fraction2"/> is null.</exception>
        /// <exception cref="ArgumentException">Numerator of the <paramref name="fraction2"/> is zero.</exception>
        public static Fraction operator /(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            if (fraction2.Numerator == 0)
                throw new ArgumentException("Numerator of the second fraction is zero.", nameof(fraction2));

            return new Fraction(fraction1.Numerator * fraction2.Denominator,
                                fraction1.Denominator * fraction2.Numerator);
        }

        /// <summary>
        /// Adds two specified <see cref="Fraction"/> objects.
        /// </summary>
        /// <param name="fraction1">The first fraction to add.</param>
        /// <param name="fraction2">The second fraction to add.</param>
        /// <returns>The sum of <paramref name="fraction1"/> and <paramref name="fraction2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fraction1"/> is null. -or-
        /// <paramref name="fraction2"/> is null.</exception>
        public static Fraction operator +(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            ReduceToCommonDenominator(fraction1, fraction2, out var numerator1, out var numerator2, out var denominator);
            return new Fraction(numerator1 + numerator2,
                                denominator);
        }

        /// <summary>
        /// Subtracts one specified <see cref="Fraction"/> object from another.
        /// </summary>
        /// <param name="fraction1">The minuend.</param>
        /// <param name="fraction2">The subtrahend.</param>
        /// <returns>The result of subtracting <paramref name="fraction2"/> from <paramref name="fraction1"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fraction1"/> is null. -or-
        /// <paramref name="fraction2"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="fraction1"/> is less than <paramref name="fraction2"/>.</exception>
        public static Fraction operator -(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            ReduceToCommonDenominator(fraction1, fraction2, out var numerator1, out var numerator2, out var denominator);
            if (numerator1 < numerator2)
                throw new ArgumentException("First fraction is less than second one.", nameof(fraction1));

            return new Fraction(numerator1 - numerator2,
                                denominator);
        }

        /// <summary>
        /// Determines if a <see cref="Fraction"/> is less than another one.
        /// </summary>
        /// <param name="fraction1">The first fraction.</param>
        /// <param name="fraction2">The second fraction.</param>
        /// <returns>true if the first fraction is less than the second, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fraction1"/> is null. -or-
        /// <paramref name="fraction2"/> is null.</exception>
        public static bool operator <(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            ReduceToCommonDenominator(fraction1, fraction2, out var numerator1, out var numerator2, out _);
            return numerator1 < numerator2;
        }

        /// <summary>
        /// Determines if a <see cref="Fraction"/> is greater than another one.
        /// </summary>
        /// <param name="fraction1">The first fraction.</param>
        /// <param name="fraction2">The second fraction.</param>
        /// <returns>true if the first fraction is greater than the second, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fraction1"/> is null. -or-
        /// <paramref name="fraction2"/> is null.</exception>
        public static bool operator >(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            ReduceToCommonDenominator(fraction1, fraction2, out var numerator1, out var numerator2, out _);
            return numerator1 > numerator2;
        }

        /// <summary>
        /// Determines if a <see cref="Fraction"/> is less than or equal to another one.
        /// </summary>
        /// <param name="fraction1">The first fraction.</param>
        /// <param name="fraction2">The second fraction.</param>
        /// <returns>true if the first fraction is less than or equal to the second, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fraction1"/> is null. -or-
        /// <paramref name="fraction2"/> is null.</exception>
        public static bool operator <=(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            ReduceToCommonDenominator(fraction1, fraction2, out var numerator1, out var numerator2, out _);
            return numerator1 <= numerator2;
        }

        /// <summary>
        /// Determines if a <see cref="Fraction"/> is greater than or equal to another one.
        /// </summary>
        /// <param name="fraction1">The first fraction.</param>
        /// <param name="fraction2">The second fraction.</param>
        /// <returns>true if the first fraction is greater than or equal to the second, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fraction1"/> is null. -or-
        /// <paramref name="fraction2"/> is null.</exception>
        public static bool operator >=(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            ReduceToCommonDenominator(fraction1, fraction2, out var numerator1, out var numerator2, out _);
            return numerator1 >= numerator2;
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
            return Equals(obj as Fraction);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Numerator.GetHashCode() ^ Denominator.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Numerator}/{Denominator}";
        }

        #endregion
    }
}
