using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class Fraction
    {
        #region Constants

        public static readonly Fraction NoFraction = new Fraction();

        public static readonly Fraction Whole = new Fraction(WholeFraction);
        public static readonly Fraction WholeDotted = new Fraction(WholeFraction, true);
        public static readonly Fraction WholeTriplet = Triplet(WholeFraction);

        public static readonly Fraction Half = new Fraction(HalfFraction);
        public static readonly Fraction HalfDotted = new Fraction(HalfFraction, true);
        public static readonly Fraction HalfTriplet = Triplet(HalfFraction);

        public static readonly Fraction Quarter = new Fraction(QuarterFraction);
        public static readonly Fraction QuarterDotted = new Fraction(QuarterFraction, true);
        public static readonly Fraction QuarterTriplet = Triplet(QuarterFraction);

        public static readonly Fraction Eighth = new Fraction(EighthFraction);
        public static readonly Fraction EighthDotted = new Fraction(EighthFraction, true);
        public static readonly Fraction EighthTriplet = Triplet(EighthFraction);

        public static readonly Fraction Sixteenth = new Fraction(SixteenthFraction);
        public static readonly Fraction SixteenthDotted = new Fraction(SixteenthFraction, true);
        public static readonly Fraction SixteenthTriplet = Triplet(SixteenthFraction);

        public static readonly Fraction ThirtySecond = new Fraction(ThirtySecondFraction);
        public static readonly Fraction ThirtySecondDotted = new Fraction(ThirtySecondFraction, true);
        public static readonly Fraction ThirtySecondTriplet = Triplet(ThirtySecondFraction);

        public static readonly Fraction SixtyFourth = new Fraction(SixtyFourthFraction);
        public static readonly Fraction SixtyFourthDotted = new Fraction(SixtyFourthFraction, true);
        public static readonly Fraction SixtyFourthTriplet = Triplet(SixtyFourthFraction);

        private const int WholeFraction = 1;
        private const int HalfFraction = 2;
        private const int QuarterFraction = 4;
        private const int EighthFraction = 8;
        private const int SixteenthFraction = 16;
        private const int ThirtySecondFraction = 32;
        private const int SixtyFourthFraction = 64;

        private const int TripletNotesCount = 3;
        private const int TripletSpaceSize = 2;

        private const int DupletNotesCount = 2;
        private const int DupletSpaceSize = 3;

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

            Simplify(ref numerator, ref denominator);

            Numerator = numerator;
            Denominator = denominator;
        }

        public Fraction(int fraction)
            : this(fraction, false)
        {
        }

        public Fraction(int fraction, bool dotted)
            : this(fraction, dotted, 1, 1)
        {
        }

        public Fraction(int fraction, int tupletNotesCount, int tupletSpaceSize)
            : this(fraction, false, tupletNotesCount, tupletSpaceSize)
        {
        }

        public Fraction(int fraction, bool dotted, int tupletNotesCount, int tupletSpaceSize)
        {
            ThrowIfArgument.IsNonpositive(nameof(fraction), fraction, "Fraction is zero or negative.");
            ThrowIfArgument.IsNonpositive(nameof(tupletNotesCount), tupletNotesCount, "Tuplet's notes count is zero or negative.");
            ThrowIfArgument.IsNonpositive(nameof(tupletSpaceSize), tupletSpaceSize, "Tuplet's space size is zero or negative.");

            //

            long numerator = 1;
            long denominator = fraction;

            if (dotted)
            {
                numerator *= 3;
                denominator *= 2;
            }

            numerator *= tupletSpaceSize;
            denominator *= tupletNotesCount;

            Simplify(ref numerator, ref denominator);

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

        public static Fraction Triplet(int fraction, bool dotted)
        {
            return new Fraction(fraction, dotted, TripletNotesCount, TripletSpaceSize);
        }

        public static Fraction Triplet(int fraction)
        {
            return Triplet(fraction, false);
        }

        public static Fraction Duplet(int fraction, bool dotted)
        {
            return new Fraction(fraction, dotted, DupletNotesCount, DupletSpaceSize);
        }

        public static Fraction Duplet(int fraction)
        {
            return Duplet(fraction, false);
        }

        internal static Fraction FromTicks(long ticks, short ticksPerQuarterNote)
        {
            var xy = MathUtilities.SolveDiophantineEquation(QuarterFraction * ticksPerQuarterNote, -ticks);
            return new Fraction(Math.Abs(xy.Item1), Math.Abs(xy.Item2));
        }

        internal long ToTicks(short ticksPerQuarterNote)
        {
            return QuarterFraction * Numerator * ticksPerQuarterNote / Denominator;
        }

        private static void Simplify(ref long numerator, ref long denominator)
        {
            var greatestCommonDivisor = MathUtilities.GreatestCommonDivisor(numerator, denominator);
            numerator /= greatestCommonDivisor;
            denominator /= greatestCommonDivisor;
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

            return new Fraction(fraction.Numerator * number, fraction.Denominator);
        }

        public static Fraction operator *(long number, Fraction fraction)
        {
            return fraction * number;
        }

        public static Fraction operator /(Fraction fraction, long number)
        {
            ThrowIfArgument.IsNull(nameof(fraction), fraction);
            ThrowIfArgument.IsNegative(nameof(number), number, "Number is negative.");

            return new Fraction(fraction.Numerator / number, fraction.Denominator);
        }

        public static Fraction operator +(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            Equalize(fraction1, fraction2, out var numerator1, out var numerator2, out var denominator);
            return new Fraction(numerator1 + numerator2, denominator);
        }

        public static Fraction operator -(Fraction fraction1, Fraction fraction2)
        {
            ThrowIfArgument.IsNull(nameof(fraction1), fraction1);
            ThrowIfArgument.IsNull(nameof(fraction2), fraction2);

            Equalize(fraction1, fraction2, out var numerator1, out var numerator2, out var denominator);
            if (numerator1 < numerator2)
                throw new ArgumentException("First fraction is less than second one.", nameof(fraction1));

            return new Fraction(numerator1 - numerator2, denominator);
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
