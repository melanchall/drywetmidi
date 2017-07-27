using System;

namespace Melanchall.DryWetMidi.Common
{
    public sealed class Fraction
    {
        #region Constants

        public static readonly Fraction NoFraction = new Fraction();

        private const long DefaultNumerator = 0;
        private const long DefaultDenominator = 1;

        #endregion

        #region Constructor

        public Fraction()
        {
        }

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

        public long Numerator { get; } = DefaultNumerator;

        public long Denominator { get; } = DefaultDenominator;

        #endregion

        #region Methods

        public bool Equals(Fraction fraction)
        {
            return this == fraction;
        }

        private static void Equalize(Fraction fraction1, Fraction fraction2, out long numerator1, out long numerator2, out long denominator)
        {
            denominator = MathUtilities.LeastCommonMultiple(fraction1.Denominator, fraction2.Denominator);

            numerator1 = fraction1.Numerator * denominator / fraction1.Denominator;
            numerator2 = fraction2.Numerator * denominator / fraction2.Denominator;
        }

        #endregion

        #region Operators

        public static bool operator ==(Fraction fraction1, Fraction fraction2)
        {
            if (ReferenceEquals(fraction1, fraction2))
                return true;

            if (ReferenceEquals(null, fraction1) || ReferenceEquals(null, fraction2))
                return false;

            Equalize(fraction1, fraction2, out var numerator1, out var numerator2, out _);
            return numerator1 == numerator2;
        }

        public static bool operator !=(Fraction fraction1, Fraction fraction2)
        {
            return !(fraction1 == fraction2);
        }

        public static Fraction operator *(Fraction fraction, long number)
        {
            ThrowIfArgument.IsNull(nameof(fraction), fraction);
            ThrowIfArgument.IsNegative(nameof(number), number, "Number is negative.");

            return new Fraction(fraction.Numerator * number,
                                fraction.Denominator);
        }

        public static Fraction operator *(long number, Fraction fraction)
        {
            return fraction * number;
        }

        public static Fraction operator *(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            return new Fraction(fraction1.Numerator * fraction2.Numerator,
                                fraction1.Denominator * fraction2.Denominator);
        }

        public static Fraction operator /(Fraction fraction, long number)
        {
            ThrowIfArgument.IsNull(nameof(fraction), fraction);
            ThrowIfArgument.IsNegative(nameof(number), number, "Number is negative.");

            return new Fraction(fraction.Numerator,
                                fraction.Denominator * number);
        }

        public static Fraction operator /(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            return new Fraction(fraction1.Numerator * fraction2.Denominator,
                                fraction1.Denominator * fraction2.Numerator);
        }

        public static Fraction operator +(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            Equalize(fraction1, fraction2, out var numerator1, out var numerator2, out var denominator);
            return new Fraction(numerator1 + numerator2,
                                denominator);
        }

        public static Fraction operator -(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            Equalize(fraction1, fraction2, out var numerator1, out var numerator2, out var denominator);
            if (numerator1 < numerator2)
                throw new ArgumentException("First fraction is less than second one.", nameof(fraction1));

            return new Fraction(numerator1 - numerator2,
                                denominator);
        }

        public static bool operator <(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            Equalize(fraction1, fraction2, out var numerator1, out var numerator2, out _);
            return numerator1 < numerator2;
        }

        public static bool operator >(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            Equalize(fraction1, fraction2, out var numerator1, out var numerator2, out _);
            return numerator1 > numerator2;
        }

        public static bool operator <=(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            Equalize(fraction1, fraction2, out var numerator1, out var numerator2, out _);
            return numerator1 <= numerator2;
        }

        public static bool operator >=(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            Equalize(fraction1, fraction2, out var numerator1, out var numerator2, out _);
            return numerator1 >= numerator2;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as Fraction);
        }

        public override int GetHashCode()
        {
            return Numerator.GetHashCode() ^ Denominator.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Numerator}/{Denominator}";
        }

        #endregion
    }
}
