using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class Fraction
    {
        #region Constants

        public static readonly Fraction NoFraction = new Fraction();

        public static readonly Fraction Whole = Create(WholeFraction);
        public static readonly Fraction WholeDotted = Create(WholeFraction, SingleDotCount);
        public static readonly Fraction WholeTriplet = CreateTriplet(WholeFraction);

        public static readonly Fraction Half = Create(HalfFraction);
        public static readonly Fraction HalfDotted = Create(HalfFraction, SingleDotCount);
        public static readonly Fraction HalfTriplet = CreateTriplet(HalfFraction);

        public static readonly Fraction Quarter = Create(QuarterFraction);
        public static readonly Fraction QuarterDotted = Create(QuarterFraction, SingleDotCount);
        public static readonly Fraction QuarterTriplet = CreateTriplet(QuarterFraction);

        public static readonly Fraction Eighth = Create(EighthFraction);
        public static readonly Fraction EighthDotted = Create(EighthFraction, SingleDotCount);
        public static readonly Fraction EighthTriplet = CreateTriplet(EighthFraction);

        public static readonly Fraction Sixteenth = Create(SixteenthFraction);
        public static readonly Fraction SixteenthDotted = Create(SixteenthFraction, SingleDotCount);
        public static readonly Fraction SixteenthTriplet = CreateTriplet(SixteenthFraction);

        public static readonly Fraction ThirtySecond = Create(ThirtySecondFraction);
        public static readonly Fraction ThirtySecondDotted = Create(ThirtySecondFraction, SingleDotCount);
        public static readonly Fraction ThirtySecondTriplet = CreateTriplet(ThirtySecondFraction);

        public static readonly Fraction SixtyFourth = Create(SixtyFourthFraction);
        public static readonly Fraction SixtyFourthDotted = Create(SixtyFourthFraction, SingleDotCount);
        public static readonly Fraction SixtyFourthTriplet = CreateTriplet(SixtyFourthFraction);

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

        private const int NoDotsCount = 0;
        private const int SingleDotCount = 1;

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

        public static Fraction Create(long fraction)
        {
            return Create(fraction, NoDotsCount);
        }

        public static Fraction Create(long fraction, int dotsCount)
        {
            return Create(fraction, dotsCount, 1, 1);
        }

        public static Fraction Create(long fraction, int tupletNotesCount, int tupletSpaceSize)
        {
            return Create(fraction, NoDotsCount, tupletNotesCount, tupletSpaceSize);
        }

        public static Fraction Create(long fraction, int dotsCount, int tupletNotesCount, int tupletSpaceSize)
        {
            ThrowIfArgument.IsNonpositive(nameof(fraction), fraction, "Fraction is zero or negative.");
            ThrowIfArgument.IsNegative(nameof(dotsCount), dotsCount, "Dots count is negative.");
            ThrowIfArgument.IsNonpositive(nameof(tupletNotesCount), tupletNotesCount, "Tuplet's notes count is zero or negative.");
            ThrowIfArgument.IsNonpositive(nameof(tupletSpaceSize), tupletSpaceSize, "Tuplet's space size is zero or negative.");

            return new Fraction(((1 << dotsCount + 1) - 1) * tupletSpaceSize,
                                fraction * (1 << dotsCount) * tupletNotesCount);
        }

        public static Fraction CreateTriplet(int fraction, int dotsCount)
        {
            return Create(fraction, dotsCount, TripletNotesCount, TripletSpaceSize);
        }

        public static Fraction CreateTriplet(int fraction)
        {
            return CreateTriplet(fraction, NoDotsCount);
        }

        public static Fraction CreateDuplet(int fraction, int dotsCount)
        {
            return Create(fraction, dotsCount, DupletNotesCount, DupletSpaceSize);
        }

        public static Fraction CreateDuplet(int fraction)
        {
            return CreateDuplet(fraction, NoDotsCount);
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
