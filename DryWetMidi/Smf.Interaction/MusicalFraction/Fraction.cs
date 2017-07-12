using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class Fraction
    {
        #region Nested classes

        private sealed class EqualizedFractions
        {
            #region Constructor

            public EqualizedFractions(long numerator1, long numerator2, long denominator)
            {
                Numerator1 = numerator1;
                Numerator2 = numerator2;
                Denominator = denominator;
            }

            #endregion

            #region Properties

            public long Numerator1 { get; }

            public long Numerator2 { get; }

            public long Denominator { get; }

            #endregion
        }

        #endregion

        #region Constructor

        public Fraction(long numerator, long denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        #endregion

        #region Properties

        public long Numerator { get; }

        public long Denominator { get; }

        #endregion

        #region Methods

        public bool Equals(Fraction fraction)
        {
            if (ReferenceEquals(null, fraction))
                return false;

            if (ReferenceEquals(this, fraction))
                return true;

            var denominator = MathUtilities.LeastCommonMultiple(Denominator, fraction.Denominator);
            return (Numerator * Denominator / denominator) ==
                   (fraction.Numerator * fraction.Denominator / denominator);
        }

        public static Fraction Sum(IEnumerable<Fraction> fractions)
        {
            if (fractions == null)
                throw new ArgumentNullException(nameof(fractions));

            if (!fractions.Any())
                return new Fraction(0, 1);

            var denominator = MathUtilities.LeastCommonMultiple(fractions.Select(f => f.Denominator));
            var numerator = fractions.Sum(f => f.Numerator * denominator / f.Denominator);

            return Simplify(new Fraction(numerator, denominator));
        }

        public static Fraction Simplify(Fraction fraction)
        {
            if (fraction == null)
                throw new ArgumentNullException(nameof(fraction));

            var greatestCommonDivisor = MathUtilities.GreatestCommonDivisor(fraction.Numerator, fraction.Denominator);
            return new Fraction(fraction.Numerator / greatestCommonDivisor,
                                fraction.Denominator / greatestCommonDivisor);
        }

        private static EqualizedFractions Equalize(Fraction fraction1, Fraction fraction2)
        {
            var denominator = MathUtilities.LeastCommonMultiple(fraction1.Denominator, fraction2.Denominator);
            return new EqualizedFractions(fraction1.Numerator * fraction1.Denominator / denominator,
                                          fraction2.Numerator * fraction2.Denominator,
                                          denominator);
        }

        #endregion

        #region Operators

        public static Fraction operator +(Fraction fraction1, Fraction fraction2)
        {
            if (fraction1 == null)
                throw new ArgumentNullException(nameof(fraction1));

            if (fraction2 == null)
                throw new ArgumentNullException(nameof(fraction2));

            var equalizedFractions = Equalize(fraction1, fraction2);
            return Simplify(new Fraction(equalizedFractions.Numerator1 + equalizedFractions.Numerator2, equalizedFractions.Denominator));
        }

        public static Fraction operator -(Fraction fraction1, Fraction fraction2)
        {
            if (fraction1 == null)
                throw new ArgumentNullException(nameof(fraction1));

            if (fraction2 == null)
                throw new ArgumentNullException(nameof(fraction2));

            var equalizedFractions = Equalize(fraction1, fraction2);
            if (equalizedFractions.Numerator1 < equalizedFractions.Numerator2)
                throw new ArgumentException("First fraction is less than second one.", nameof(fraction1));

            return Simplify(new Fraction(equalizedFractions.Numerator1 - equalizedFractions.Numerator2, equalizedFractions.Denominator));
        }

        public static bool operator <(Fraction fraction1, Fraction fraction2)
        {
            if (fraction1 == null)
                throw new ArgumentNullException(nameof(fraction1));

            if (fraction2 == null)
                throw new ArgumentNullException(nameof(fraction2));

            var equalizedFractions = Equalize(fraction1, fraction2);
            return equalizedFractions.Numerator1 < equalizedFractions.Numerator2;
        }

        public static bool operator >(Fraction fraction1, Fraction fraction2)
        {
            if (fraction1 == null)
                throw new ArgumentNullException(nameof(fraction1));

            if (fraction2 == null)
                throw new ArgumentNullException(nameof(fraction2));

            var equalizedFractions = Equalize(fraction1, fraction2);
            return equalizedFractions.Numerator1 > equalizedFractions.Numerator2;
        }

        public static bool operator <=(Fraction fraction1, Fraction fraction2)
        {
            if (fraction1 == null)
                throw new ArgumentNullException(nameof(fraction1));

            if (fraction2 == null)
                throw new ArgumentNullException(nameof(fraction2));

            var equalizedFractions = Equalize(fraction1, fraction2);
            return equalizedFractions.Numerator1 <= equalizedFractions.Numerator2;
        }

        public static bool operator >=(Fraction fraction1, Fraction fraction2)
        {
            if (fraction1 == null)
                throw new ArgumentNullException(nameof(fraction1));

            if (fraction2 == null)
                throw new ArgumentNullException(nameof(fraction2));

            var equalizedFractions = Equalize(fraction1, fraction2);
            return equalizedFractions.Numerator1 >= equalizedFractions.Numerator2;
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
