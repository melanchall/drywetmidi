using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Helper class to get an instance of the <see cref="Fraction"/> by musical parameters
    /// like dots count and tuplet definition.
    /// </summary>
    public static class MusicalFraction
    {
        #region Constants

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the whole length.
        /// </summary>
        public static readonly Fraction Whole = Create(WholeFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the single dotted whole length.
        /// </summary>
        public static readonly Fraction WholeDotted = CreateSingleDotted(WholeFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the triplet whole length.
        /// </summary>
        public static readonly Fraction WholeTriplet = CreateTriplet(WholeFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the half length.
        /// </summary>
        public static readonly Fraction Half = Create(HalfFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the single dotted half length.
        /// </summary>
        public static readonly Fraction HalfDotted = CreateSingleDotted(HalfFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the triplet half length.
        /// </summary>
        public static readonly Fraction HalfTriplet = CreateTriplet(HalfFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the quarter length.
        /// </summary>
        public static readonly Fraction Quarter = Create(QuarterFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the single dotted quarter length.
        /// </summary>
        public static readonly Fraction QuarterDotted = CreateSingleDotted(QuarterFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the triplet quarter length.
        /// </summary>
        public static readonly Fraction QuarterTriplet = CreateTriplet(QuarterFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the eighth length.
        /// </summary>
        public static readonly Fraction Eighth = Create(EighthFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the single dotted eighth length.
        /// </summary>
        public static readonly Fraction EighthDotted = CreateSingleDotted(EighthFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the triplet eighth length.
        /// </summary>
        public static readonly Fraction EighthTriplet = CreateTriplet(EighthFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the sixteenth length.
        /// </summary>
        public static readonly Fraction Sixteenth = Create(SixteenthFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the single dotted sixteenth length.
        /// </summary>
        public static readonly Fraction SixteenthDotted = CreateSingleDotted(SixteenthFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the triplet sixteenth length.
        /// </summary>
        public static readonly Fraction SixteenthTriplet = CreateTriplet(SixteenthFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the thirty-second length.
        /// </summary>
        public static readonly Fraction ThirtySecond = Create(ThirtySecondFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the single dotted thirty-second length.
        /// </summary>
        public static readonly Fraction ThirtySecondDotted = CreateSingleDotted(ThirtySecondFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the triplet thirty-second length.
        /// </summary>
        public static readonly Fraction ThirtySecondTriplet = CreateTriplet(ThirtySecondFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the sixty-fourth length.
        /// </summary>
        public static readonly Fraction SixtyFourth = Create(SixtyFourthFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the single dotted sixty-fourth length.
        /// </summary>
        public static readonly Fraction SixtyFourthDotted = CreateSingleDotted(SixtyFourthFraction);

        /// <summary>
        /// <see cref="Fraction"/> that corresponds to the triplet sixty-fourth length.
        /// </summary>
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

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified fraction
        /// of the whole length, e.g. 1/8 for eighth.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <returns><see cref="Fraction"/> that represents fraction of the whole length, e.g. 1/8.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative.</exception>
        public static Fraction Create(long fraction)
        {
            return CreateDotted(fraction, NoDotsCount);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified dotted fraction
        /// of the whole length, e.g. 3/2 for single dotted whole.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <param name="dotsCount">Count of dots attached to the fraction.</param>
        /// <returns><see cref="Fraction"/> that represents fraction of the whole length, e.g. 3/2 for
        /// single dotted whole.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative. -or-
        /// <paramref name="dotsCount"/> is negative..</exception>
        public static Fraction CreateDotted(long fraction, int dotsCount)
        {
            return CreateDottedTuplet(fraction, dotsCount, NoTupletNotesCount, NoTupletSpaceSize);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified tuplet fraction,
        /// e.g. 1/12 for triplet eighth.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <param name="tupletNotesCount">Count of notes a tuplet made up from, e.g. 3 for triplet.</param>
        /// <param name="tupletSpaceSize">Size of tuplet's space which is count of regular notes
        /// that have total length equal to the tuplet, e.g. 2 for triplet.</param>
        /// <returns><see cref="Fraction"/> that represents fraction of the whole length, e.g. 1/12 for triplet eighth.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative. -or-
        /// <paramref name="tupletNotesCount"/> is zero or negative. -or- <paramref name="tupletSpaceSize"/> is
        /// zero or negative.</exception>
        public static Fraction CreateTuplet(long fraction, int tupletNotesCount, int tupletSpaceSize)
        {
            return CreateDottedTuplet(fraction, NoDotsCount, tupletNotesCount, tupletSpaceSize);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified fraction
        /// of the whole length. Fraction can have dots and can represent tuplet's note length.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <param name="dotsCount">Count of dots attached to the fraction.</param>
        /// <param name="tupletNotesCount">Count of notes a tuplet made up from, e.g. 3 for triplet.</param>
        /// <param name="tupletSpaceSize">Size of tuplet's space which is count of regular notes
        /// that have total length equal to the tuplet, e.g. 2 for triplet.</param>
        /// <returns><see cref="Fraction"/> that represents fraction of the whole length, e.g. 3/2 for dotted whole.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative. -or-
        /// <paramref name="dotsCount"/> is negative. -or- <paramref name="tupletNotesCount"/> is zero or
        /// negative. -or- <paramref name="tupletSpaceSize"/> is zero or negative.</exception>
        public static Fraction CreateDottedTuplet(long fraction, int dotsCount, int tupletNotesCount, int tupletSpaceSize)
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

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified dotted
        /// triplet fraction, e.g. 3/24 for single dotted triplet eighth.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <param name="dotsCount">Count of dots attached to the fraction.</param>
        /// <returns><see cref="Fraction"/> that represents dotted triplet fraction of the whole length, e.g.
        /// 3/24 for single dotted triplet eighth.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative. -or-
        /// <paramref name="dotsCount"/> is negative.</exception>
        public static Fraction CreateDottedTriplet(int fraction, int dotsCount)
        {
            return CreateDottedTuplet(fraction, dotsCount, TripletNotesCount, TripletSpaceSize);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified triplet fraction,
        /// e.g. 1/12 for triplet eighth.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <returns><see cref="Fraction"/> that represents triplet fraction of the whole length, e.g.
        /// 1/12 for triplet eighth.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative.</exception>
        public static Fraction CreateTriplet(int fraction)
        {
            return CreateDottedTriplet(fraction, NoDotsCount);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified dotted
        /// duplet fraction, e.g. 9/32 for single dotted duplet eighth.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <param name="dotsCount">Count of dots attached to the fraction.</param>
        /// <returns><see cref="Fraction"/> that represents dotted duplet fraction of the whole length, e.g.
        /// 9/32 for single dotted duplet eighth.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative. -or-
        /// <paramref name="dotsCount"/> is negative.</exception>
        public static Fraction CreateDottedDuplet(int fraction, int dotsCount)
        {
            return CreateDottedTuplet(fraction, dotsCount, DupletNotesCount, DupletSpaceSize);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified duplet fraction,
        /// e.g. 3/16 for duplet eighth.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <returns><see cref="Fraction"/> that represents duplet fraction of the whole length, e.g.
        /// 3/16 for duplet eighth.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative.</exception>
        public static Fraction CreateDuplet(int fraction)
        {
            return CreateDottedDuplet(fraction, NoDotsCount);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified single dotted fraction,
        /// e.g. 3/2 for single dotted whole.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <returns><see cref="Fraction"/> that represents single dotted fraction of the whole length, e.g.
        /// 3/2 for single dotted whole.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative.</exception>
        public static Fraction CreateSingleDotted(int fraction)
        {
            return CreateDotted(fraction, SingleDotCount);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified single dotted
        /// tuplet fraction, e.g. 1/8 for single dotted triplet eight.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <param name="tupletNotesCount">Count of notes a tuplet made up from, e.g. 3 for triplet.</param>
        /// <param name="tupletSpaceSize">Size of tuplet's space which is count of regular notes
        /// that have total length equal to the tuplet, e.g. 2 for triplet.</param>
        /// <returns><see cref="Fraction"/> that represents single dotted tuplet fraction, e.g. 1/8 for single dotted
        /// triplet eight.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative. -or-
        /// <paramref name="tupletNotesCount"/> is zero or negative. -or- <paramref name="tupletSpaceSize"/> is
        /// zero or negative.</exception>
        public static Fraction CreateSingleDottedTuplet(int fraction, int tupletNotesCount, int tupletSpaceSize)
        {
            return CreateDottedTuplet(fraction, SingleDotCount, tupletNotesCount, tupletSpaceSize);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified double dotted fraction,
        /// e.g. 7/4 for double dotted whole.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <returns><see cref="Fraction"/> that represents double dotted fraction of the whole length, e.g.
        /// 7/4 for double dotted whole.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative.</exception>
        public static Fraction CreateDoubleDotted(int fraction)
        {
            return CreateDotted(fraction, DoubleDotCount);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Fraction"/> that represents the specified double dotted
        /// tuplet fraction, e.g. 7/48 for double dotted triplet eight.
        /// </summary>
        /// <param name="fraction">Fraction of the whole length, e.g. 8 for 1/8.</param>
        /// <param name="tupletNotesCount">Count of notes a tuplet made up from, e.g. 3 for triplet.</param>
        /// <param name="tupletSpaceSize">Size of tuplet's space which is count of regular notes
        /// that have total length equal to the tuplet, e.g. 2 for triplet.</param>
        /// <returns><see cref="Fraction"/> that represents double dotted tuplet fraction, e.g. 7/48 for double dotted
        /// triplet eight.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is zero or negative. -or-
        /// <paramref name="tupletNotesCount"/> is zero or negative. -or- <paramref name="tupletSpaceSize"/> is
        /// zero or negative.</exception>
        public static Fraction CreateDoubleDottedTuplet(int fraction, int tupletNotesCount, int tupletSpaceSize)
        {
            return CreateDottedTuplet(fraction, DoubleDotCount, tupletNotesCount, tupletSpaceSize);
        }

        #endregion
    }
}
