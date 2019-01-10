using System;
using System.Collections.Generic;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Represents a musical interval in terms of half steps number.
    /// </summary>
    public sealed class Interval
    {
        #region Fields

        private static readonly Dictionary<SevenBitNumber, Dictionary<IntervalDirection, Interval>> _cache =
            new Dictionary<SevenBitNumber, Dictionary<IntervalDirection, Interval>>();

        #endregion

        #region Constants

        /// <summary>
        /// Interval of zero half steps up.
        /// </summary>
        public static readonly Interval Zero = FromHalfSteps(0);

        /// <summary>
        /// Interval of one half step up.
        /// </summary>
        public static readonly Interval One = FromHalfSteps(1);

        /// <summary>
        /// Interval of two half steps up.
        /// </summary>
        public static readonly Interval Two = FromHalfSteps(2);

        /// <summary>
        /// Interval of three half steps up.
        /// </summary>
        public static readonly Interval Three = FromHalfSteps(3);

        /// <summary>
        /// Interval of four half steps up.
        /// </summary>
        public static readonly Interval Four = FromHalfSteps(4);

        /// <summary>
        /// Interval of five half steps up.
        /// </summary>
        public static readonly Interval Five = FromHalfSteps(5);

        /// <summary>
        /// Interval of six half steps up.
        /// </summary>
        public static readonly Interval Six = FromHalfSteps(6);

        /// <summary>
        /// Interval of seven half steps up.
        /// </summary>
        public static readonly Interval Seven = FromHalfSteps(7);

        /// <summary>
        /// Interval of eight half steps up.
        /// </summary>
        public static readonly Interval Eight = FromHalfSteps(8);

        /// <summary>
        /// Interval of nine half steps up.
        /// </summary>
        public static readonly Interval Nine = FromHalfSteps(9);

        /// <summary>
        /// Interval of ten half steps up.
        /// </summary>
        public static readonly Interval Ten = FromHalfSteps(10);

        /// <summary>
        /// Interval of eleven half steps up.
        /// </summary>
        public static readonly Interval Eleven = FromHalfSteps(11);

        /// <summary>
        /// Interval of twelve half steps up (one octave up).
        /// </summary>
        public static readonly Interval Twelve = FromHalfSteps(12);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Interval"/> with the
        /// specified interval and its direction.
        /// </summary>
        /// <param name="size">The size of interval as a number of half steps away.</param>
        /// <param name="direction">The direction of an interval (up or down).</param>
        private Interval(SevenBitNumber size, IntervalDirection direction)
        {
            Size = size;
            Direction = direction;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the size of interval represented by the current <see cref="Interval"/> as a
        /// number of half steps away.
        /// </summary>
        public SevenBitNumber Size { get; }

        /// <summary>
        /// Gets the direction of the interval represented by the current <see cref="Interval"/>.
        /// </summary>
        public IntervalDirection Direction { get; }

        /// <summary>
        /// Gets signed half steps number which represents an interval of the current <see cref="Interval"/>.
        /// </summary>
        public int HalfSteps => Direction == IntervalDirection.Up
            ? Size
            : -Size;

        #endregion

        #region Methods

        /// <summary>
        /// Returns upward version of the current <see cref="Interval"/>.
        /// </summary>
        /// <returns>An upward version of the current <see cref="Interval"/>.</returns>
        public Interval Up()
        {
            return Get(Size, IntervalDirection.Up);
        }

        /// <summary>
        /// Returns downward version of the current <see cref="Interval"/>.
        /// </summary>
        /// <returns>A downward version of the current <see cref="Interval"/>.</returns>
        public Interval Down()
        {
            return Get(Size, IntervalDirection.Down);
        }

        /// <summary>
        /// Returns an <see cref="Interval"/> by the specified half steps number and
        /// interval's direction.
        /// </summary>
        /// <param name="intervalSize">The size of an interval as a number of half steps away.</param>
        /// <param name="direction">The direction of an interval (up or down).</param>
        /// <returns>An <see cref="Interval"/> with the specified interval and direction.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="direction"/> specified an
        /// invalid value.</exception>
        public static Interval Get(SevenBitNumber intervalSize, IntervalDirection direction)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(direction), direction);

            Dictionary<IntervalDirection, Interval> intervals;
            if (!_cache.TryGetValue(intervalSize, out intervals))
                _cache.Add(intervalSize, intervals = new Dictionary<IntervalDirection, Interval>());

            Interval cachedInterval;
            if (!intervals.TryGetValue(direction, out cachedInterval))
                intervals.Add(direction, cachedInterval = new Interval(intervalSize, direction));

            return cachedInterval;
        }

        /// <summary>
        /// Returns an upward <see cref="Interval"/> by the specified half steps number.
        /// </summary>
        /// <param name="intervalSize">The size of an interval as a number of half steps away.</param>
        /// <returns>An upward <see cref="Interval"/> with the specified interval.</returns>
        public static Interval GetUp(SevenBitNumber intervalSize)
        {
            return Get(intervalSize, IntervalDirection.Up);
        }

        /// <summary>
        /// Returns a downward <see cref="Interval"/> by the specified half steps number.
        /// </summary>
        /// <param name="intervalSize">The size of an interval as a number of half steps away.</param>
        /// <returns>A downward <see cref="Interval"/> with the specified interval.</returns>
        public static Interval GetDown(SevenBitNumber intervalSize)
        {
            return Get(intervalSize, IntervalDirection.Down);
        }

        /// <summary>
        /// Returns an <see cref="Interval"/> by the specified signed number of
        /// half steps where negative one means downward interval.
        /// </summary>
        /// <param name="halfSteps">The number of half steps.</param>
        /// <returns>An <see cref="Interval"/> represented by the <paramref name="halfSteps"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfSteps"/> is out of range
        /// (result interval is out of the [-127,127] range).</exception>
        public static Interval FromHalfSteps(int halfSteps)
        {
            ThrowIfArgument.IsOutOfRange(nameof(halfSteps),
                                         halfSteps,
                                         -SevenBitNumber.MaxValue,
                                         SevenBitNumber.MaxValue,
                                         "Half steps number is out of range.");

            return Get((SevenBitNumber)Math.Abs(halfSteps),
                       Math.Sign(halfSteps) < 0 ? IntervalDirection.Down : IntervalDirection.Up);
        }

        /// <summary>
        /// Converts the string representation of a musical interval to its <see cref="Interval"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing an interval to convert.</param>
        /// <param name="interval">When this method returns, contains the <see cref="Interval"/>
        /// equivalent of the musical interval contained in <paramref name="input"/>, if the conversion succeeded,
        /// or null if the conversion failed. The conversion fails if the <paramref name="input"/> is null or
        /// <see cref="string.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns>true if <paramref name="input"/> was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string input, out Interval interval)
        {
            return IntervalParser.TryParse(input, out interval).Status == ParsingStatus.Parsed;
        }

        /// <summary>
        /// Converts the string representation of a musical interval to its <see cref="Scale"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing an interval to convert.</param>
        /// <returns>A <see cref="Scale"/> equivalent to the musical interval contained in <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is null or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static Interval Parse(string input)
        {
            Interval interval;
            var parsingResult = IntervalParser.TryParse(input, out interval);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return interval;

            throw parsingResult.Exception;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Converts the value of a <see cref="Interval"/> to a <see cref="int"/>.
        /// </summary>
        /// <param name="interval"><see cref="Interval"/> object to convert to an <see cref="int"/>.</param>
        public static implicit operator int(Interval interval)
        {
            return interval.HalfSteps;
        }

        /// <summary>
        /// Converts the value of a <see cref="SevenBitNumber"/> to a <see cref="Interval"/>.
        /// </summary>
        /// <param name="interval"><see cref="SevenBitNumber"/> object to convert to an <see cref="Interval"/>.</param>
        public static implicit operator Interval(SevenBitNumber interval)
        {
            return GetUp(interval);
        }

        /// <summary>
        /// Determines if two <see cref="Interval"/> objects are equal.
        /// </summary>
        /// <param name="interval1">The first <see cref="Interval"/> to compare.</param>
        /// <param name="interval2">The second <see cref="Interval"/> to compare.</param>
        /// <returns>true if the intervals are equal, false otherwise.</returns>
        public static bool operator ==(Interval interval1, Interval interval2)
        {
            if (ReferenceEquals(interval1, interval2))
                return true;

            if (ReferenceEquals(null, interval1) || ReferenceEquals(null, interval2))
                return false;

            return interval1.HalfSteps == interval2.HalfSteps;
        }

        /// <summary>
        /// Determines if two <see cref="Interval"/> objects are not equal.
        /// </summary>
        /// <param name="interval1">The first <see cref="Interval"/> to compare.</param>
        /// <param name="interval2">The second <see cref="Interval"/> to compare.</param>
        /// <returns>false if the intervals are equal, true otherwise.</returns>
        public static bool operator !=(Interval interval1, Interval interval2)
        {
            return !(interval1 == interval2);
        }

        /// <summary>
        /// Adds the specified number of half steps to an <see cref="Interval"/>.
        /// </summary>
        /// <param name="interval">The interval to add half steps to.</param>
        /// <param name="halfSteps">The number of half steps to add to the <paramref name="interval"/>.</param>
        /// <returns>The <see cref="Interval"/> which is the <paramref name="interval"/>
        /// shifted by the <paramref name="halfSteps"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfSteps"/> is out of range
        /// (result interval is out of the [-127,127] range).</exception>
        public static Interval operator +(Interval interval, int halfSteps)
        {
            ThrowIfArgument.IsNull(nameof(interval), interval);

            return FromHalfSteps(interval.HalfSteps + halfSteps);
        }

        /// <summary>
        /// Subtracts the specified number of half steps from an <see cref="Interval"/>.
        /// </summary>
        /// <param name="interval">The interval to subtract half steps from.</param>
        /// <param name="halfSteps">The number of half steps to subtract from the <paramref name="interval"/>.</param>
        /// <returns>The <see cref="Interval"/> which is the <paramref name="interval"/>
        /// shifted by the <paramref name="halfSteps"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfSteps"/> is out of range
        /// (result interval is out of the [-127,127] range).</exception>
        public static Interval operator -(Interval interval, int halfSteps)
        {
            ThrowIfArgument.IsNull(nameof(interval), interval);

            return FromHalfSteps(interval.HalfSteps - halfSteps);
        }

        /// <summary>
        /// Stretches the specified <see cref="Interval"/> by multiplying it by an integer number.
        /// </summary>
        /// <param name="interval">The interval to stretch.</param>
        /// <param name="multiplier">The number to multiply the <paramref name="interval"/> by.</param>
        /// <returns>The <see cref="Interval"/> which is the <paramref name="interval"/>
        /// stretched by the <paramref name="multiplier"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="multiplier"/> is out of range
        /// (result interval is out of the [-127,127] range).</exception>
        public static Interval operator *(Interval interval, int multiplier)
        {
            ThrowIfArgument.IsNull(nameof(interval), interval);

            return FromHalfSteps(interval.HalfSteps * multiplier);
        }

        /// <summary>
        /// Shrinks the specified <see cref="Interval"/> by dividing it by an integer number.
        /// </summary>
        /// <param name="interval">The interval to shrink.</param>
        /// <param name="divisor">The number to divide the <paramref name="interval"/> by.</param>
        /// <returns>The <see cref="Interval"/> which is the <paramref name="interval"/>
        /// shrinked by the <paramref name="divisor"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is zero.</exception>
        public static Interval operator /(Interval interval, int divisor)
        {
            ThrowIfArgument.IsNull(nameof(interval), interval);

            if (divisor == 0)
                throw new ArgumentOutOfRangeException(nameof(divisor), divisor, "Divisor is zero.");

            return FromHalfSteps(interval.HalfSteps / divisor);
        }

        /// <summary>
        /// Returns upward version of the specified <see cref="Interval"/>.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> to get upward version of.</param>
        /// <returns>An upward version of the <paramref name="interval"/>.</returns>
        /// <remarks>
        /// This operator produces the same result as the <see cref="Up"/> method.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is null.</exception>
        public static Interval operator +(Interval interval)
        {
            ThrowIfArgument.IsNull(nameof(interval), interval);

            return interval.Up();
        }

        /// <summary>
        /// Returns downward version of the specified <see cref="Interval"/>.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> to get downward version of.</param>
        /// <returns>A downward version of the <paramref name="interval"/>.</returns>
        /// <remarks>
        /// This operator produces the same result as the <see cref="Down"/> method.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is null.</exception>
        public static Interval operator -(Interval interval)
        {
            ThrowIfArgument.IsNull(nameof(interval), interval);

            return interval.Down();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{(Direction == IntervalDirection.Up ? "+" : "-")}{Size}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as Interval);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HalfSteps.GetHashCode();
        }

        #endregion
    }
}
