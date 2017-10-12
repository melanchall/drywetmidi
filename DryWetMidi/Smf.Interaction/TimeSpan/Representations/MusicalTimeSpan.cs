using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MusicalTimeSpan : ITimeSpan
    {
        #region Constants

        /// <summary>
        /// <see cref="MusicalTimeSpan"/> that corresponds to the whole length.
        /// </summary>
        public static readonly MusicalTimeSpan Whole = new MusicalTimeSpan(WholeFraction);

        /// <summary>
        /// <see cref="MusicalTimeSpan"/> that corresponds to the half length.
        /// </summary>
        public static readonly MusicalTimeSpan Half = new MusicalTimeSpan(HalfFraction);

        /// <summary>
        /// <see cref="MusicalTimeSpan"/> that corresponds to the quarter length.
        /// </summary>
        public static readonly MusicalTimeSpan Quarter = new MusicalTimeSpan(QuarterFraction);

        /// <summary>
        /// <see cref="MusicalTimeSpan"/> that corresponds to the eighth length.
        /// </summary>
        public static readonly MusicalTimeSpan Eighth = new MusicalTimeSpan(EighthFraction);

        /// <summary>
        /// <see cref="MusicalTimeSpan"/> that corresponds to the sixteenth length.
        /// </summary>
        public static readonly MusicalTimeSpan Sixteenth = new MusicalTimeSpan(SixteenthFraction);

        /// <summary>
        /// <see cref="MusicalTimeSpan"/> that corresponds to the thirty-second length.
        /// </summary>
        public static readonly MusicalTimeSpan ThirtySecond = new MusicalTimeSpan(ThirtySecondFraction);

        /// <summary>
        /// <see cref="MusicalTimeSpan"/> that corresponds to the sixty-fourth length.
        /// </summary>
        public static readonly MusicalTimeSpan SixtyFourth = new MusicalTimeSpan(SixtyFourthFraction);

        private const long ZeroTimeSpanNumerator = 0;
        private const long ZeroTimeSpanDenominator = 1;

        private const long FractionNumerator = 1;

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

        private const int NumberOfDigitsAfterDecimalPoint = 3;
        private static readonly int FractionPartMultiplier = (int)Math.Pow(10, NumberOfDigitsAfterDecimalPoint);

        #endregion

        #region Constructor

        public MusicalTimeSpan()
            : this(ZeroTimeSpanNumerator, ZeroTimeSpanDenominator)
        {
        }

        public MusicalTimeSpan(long fraction)
            : this(FractionNumerator, fraction)
        {
        }

        public MusicalTimeSpan(long numerator, long denominator)
        {
            ThrowIfArgument.IsNegative(nameof(numerator), numerator, "Numerator is negative.");
            ThrowIfArgument.IsNonpositive(nameof(denominator), denominator, "Denominator is negative.");

            var greatestCommonDivisor = MathUtilities.GreatestCommonDivisor(numerator, denominator);

            Numerator = numerator / greatestCommonDivisor;
            Denominator = denominator / greatestCommonDivisor;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the numerator of the current <see cref="MusicalTimeSpan"/>.
        /// </summary>
        public long Numerator { get; } = ZeroTimeSpanNumerator;

        /// <summary>
        /// Gets the denominator of the current <see cref="MusicalTimeSpan"/>.
        /// </summary>
        public long Denominator { get; } = ZeroTimeSpanDenominator;

        #endregion

        #region Methods

        public MusicalTimeSpan Dotted(int dotsCount)
        {
            ThrowIfArgument.IsNegative(nameof(dotsCount), dotsCount, "Dots count is negative.");

            return new MusicalTimeSpan(Numerator * ((1 << dotsCount + 1) - 1),
                                       Denominator * (1 << dotsCount));
        }

        public MusicalTimeSpan SingleDotted()
        {
            return Dotted(SingleDotCount);
        }

        public MusicalTimeSpan DoubleDotted()
        {
            return Dotted(DoubleDotCount);
        }

        public MusicalTimeSpan Tuplet(int tupletNotesCount, int tupletSpaceSize)
        {
            ThrowIfArgument.IsNonpositive(nameof(tupletNotesCount), tupletNotesCount, "Tuplet's notes count is zero or negative.");
            ThrowIfArgument.IsNonpositive(nameof(tupletSpaceSize), tupletSpaceSize, "Tuplet's space size is zero or negative.");

            return new MusicalTimeSpan(Numerator * tupletSpaceSize,
                                       Denominator * tupletNotesCount);
        }

        public MusicalTimeSpan Triplet()
        {
            return Tuplet(TripletNotesCount, TripletSpaceSize);
        }

        public MusicalTimeSpan Duplet()
        {
            return Tuplet(DupletNotesCount, DupletSpaceSize);
        }

        public static bool TryParse(string input, out MusicalTimeSpan timeSpan)
        {
            return MusicalTimeSpanParser.TryParse(input, out timeSpan).Status == ParsingStatus.Parsed;
        }

        public static MusicalTimeSpan Parse(string input)
        {
            var parsingResult = MusicalTimeSpanParser.TryParse(input, out var timeSpan);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return timeSpan;

            throw parsingResult.Exception;
        }

        /// <summary>
        /// Reduces the specified musical time spans to the common denominator.
        /// </summary>
        /// <param name="fraction1">First time span.</param>
        /// <param name="fraction2">Second time span.</param>
        /// <param name="numerator1">Numerator of the reduced first time span.</param>
        /// <param name="numerator2">Numerator of the reduced second time span.</param>
        /// <param name="denominator">Common denominator of reduced time spans.</param>
        private static void ReduceToCommonDenominator(MusicalTimeSpan fraction1,
                                                      MusicalTimeSpan fraction2,
                                                      out long numerator1,
                                                      out long numerator2,
                                                      out long denominator)
        {
            denominator = MathUtilities.LeastCommonMultiple(fraction1.Denominator, fraction2.Denominator);

            numerator1 = fraction1.Numerator * denominator / fraction1.Denominator;
            numerator2 = fraction2.Numerator * denominator / fraction2.Denominator;
        }

        #endregion

        #region Operators

        public static bool operator ==(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, timeSpan2))
                return true;

            if (ReferenceEquals(null, timeSpan1) || ReferenceEquals(null, timeSpan2))
                return false;

            ReduceToCommonDenominator(timeSpan1, timeSpan2, out var numerator1, out var numerator2, out _);
            return numerator1 == numerator2;
        }

        public static bool operator !=(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        public static MusicalTimeSpan operator *(MusicalTimeSpan timeSpan, long number)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfArgument.IsNegative(nameof(number), number, "Number is negative.");

            return new MusicalTimeSpan(timeSpan.Numerator * number,
                                       timeSpan.Denominator);
        }

        public static MusicalTimeSpan operator *(long number, MusicalTimeSpan timeSpan)
        {
            return timeSpan * number;
        }

        public static MusicalTimeSpan operator /(MusicalTimeSpan timeSpan, long number)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfArgument.IsNonpositive(nameof(number), number, "Number is zero or negative.");

            return new MusicalTimeSpan(timeSpan.Numerator,
                                       timeSpan.Denominator * number);
        }

        public static MusicalTimeSpan operator +(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            ReduceToCommonDenominator(timeSpan1, timeSpan2, out var numerator1, out var numerator2, out var denominator);
            return new MusicalTimeSpan(numerator1 + numerator2, denominator);
        }

        public static MusicalTimeSpan operator -(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            ReduceToCommonDenominator(timeSpan1, timeSpan2, out var numerator1, out var numerator2, out var denominator);
            if (numerator1 < numerator2)
                throw new ArgumentException("First time span is less than second one.", nameof(timeSpan1));

            return new MusicalTimeSpan(numerator1 - numerator2, denominator);
        }

        public static bool operator <(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            ReduceToCommonDenominator(timeSpan1, timeSpan2, out var numerator1, out var numerator2, out _);
            return numerator1 < numerator2;
        }

        public static bool operator >(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            ReduceToCommonDenominator(timeSpan1, timeSpan2, out var numerator1, out var numerator2, out _);
            return numerator1 > numerator2;
        }

        public static bool operator <=(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            ReduceToCommonDenominator(timeSpan1, timeSpan2, out var numerator1, out var numerator2, out _);
            return numerator1 <= numerator2;
        }

        public static bool operator >=(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            ReduceToCommonDenominator(timeSpan1, timeSpan2, out var numerator1, out var numerator2, out _);
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
            return this == (obj as MusicalTimeSpan);
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

        #region ITimeSpan

        public ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var musicalTimeSpan = timeSpan as MusicalTimeSpan;
            return musicalTimeSpan != null
                ? this + musicalTimeSpan
                : TimeSpanUtilities.Add(this, timeSpan, mode);
        }

        public ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var musicalTimeSpan = timeSpan as MusicalTimeSpan;
            return musicalTimeSpan != null
                ? this - musicalTimeSpan
                : TimeSpanUtilities.Subtract(this, timeSpan, mode);
        }

        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MusicalTimeSpan((long)Math.Round(Numerator * Math.Round(multiplier, NumberOfDigitsAfterDecimalPoint) * FractionPartMultiplier),
                                       Denominator * FractionPartMultiplier);
        }

        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

            return new MusicalTimeSpan(Numerator * FractionPartMultiplier,
                                       (long)Math.Round(Denominator * Math.Round(divisor, NumberOfDigitsAfterDecimalPoint) * FractionPartMultiplier));
        }

        #endregion
    }
}
