using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class MusicalFraction
    {
        #region Constants

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

        private const int NoTupletNotesCount = 1;
        private const int NoTupletSpaceSize = 1;

        private const int NoDotsCount = 0;
        private const int SingleDotCount = 1;
        private const int DoubleDotCount = 2;

        #endregion

        #region Methods

        public static Fraction Create(long fraction)
        {
            return Create(fraction, NoDotsCount);
        }

        public static Fraction Create(long fraction, int dotsCount)
        {
            return Create(fraction, dotsCount, NoTupletNotesCount, NoTupletSpaceSize);
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

            // [1] create simple fraction:
            //     f = 1 / fraction
            //
            // [2] add dots:
            //     f = f * (2 ^ (dotsCount + 1) - 1) / 2 ^ dotsCount
            //
            // [3] make tuplet:
            //     f = f * tupletSpaceSize / tupletNotesCount

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

        public static Fraction CreateSingleDotted(int fraction)
        {
            return Create(fraction, SingleDotCount);
        }

        public static Fraction CreateSingleDotted(int fraction, int tupletNotesCount, int tupletSpaceSize)
        {
            return Create(fraction, SingleDotCount, tupletNotesCount, tupletSpaceSize);
        }

        public static Fraction CreateDoubleDotted(int fraction)
        {
            return Create(fraction, DoubleDotCount);
        }

        public static Fraction CreateDoubleDotted(int fraction, int tupletNotesCount, int tupletSpaceSize)
        {
            return Create(fraction, DoubleDotCount, tupletNotesCount, tupletSpaceSize);
        }

        #endregion
    }
}
