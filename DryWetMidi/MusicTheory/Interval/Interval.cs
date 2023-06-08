using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Represents a musical interval in terms of half steps number.
    /// </summary>
    public sealed class Interval : IComparable<Interval>
    {
        #region Fields

        private static readonly Dictionary<SevenBitNumber, Dictionary<IntervalDirection, Interval>> Cache =
            new Dictionary<SevenBitNumber, Dictionary<IntervalDirection, Interval>>();

        private IReadOnlyCollection<IntervalDefinition> _intervalDefinitions;

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

        private static readonly Dictionary<IntervalQuality, Dictionary<int, int>> IntervalsHalfTones =
            new Dictionary<IntervalQuality, Dictionary<int, int>>
            {
                [IntervalQuality.Perfect] = new Dictionary<int, int>
                {
                    [1] = 0, [4] = 5, [5] = 7, [8] = 12
                },
                [IntervalQuality.Minor] = new Dictionary<int, int>
                {
                    [2] = 1, [3] = 3, [6] = 8, [7] = 10
                },
                [IntervalQuality.Major] = new Dictionary<int, int>
                {
                    [2] = 2, [3] = 4, [6] = 9, [7] = 11
                },
                [IntervalQuality.Diminished] = new Dictionary<int, int>
                {
                    [1] = -1, [2] = 0, [3] = 2, [4] = 4, [5] = 6, [6] = 7, [7] = 9, [8] = 11
                },
                [IntervalQuality.Augmented] = new Dictionary<int, int>
                {
                    [1] = 1, [2] = 3, [3] = 5, [4] = 6, [5] = 8, [6] = 10, [7] = 12
                }
            };

        private static readonly IntervalQuality?[] QualitiesPattern = new IntervalQuality?[]
        {
            IntervalQuality.Perfect,
            IntervalQuality.Minor,
            IntervalQuality.Major,
            IntervalQuality.Minor,
            IntervalQuality.Major,
            IntervalQuality.Perfect,
            null,
            IntervalQuality.Perfect,
            IntervalQuality.Minor,
            IntervalQuality.Major,
            IntervalQuality.Minor,
            IntervalQuality.Major,
        };

        private static readonly Dictionary<int, IntervalQuality> AdditionalQualitiesPattern = new Dictionary<int, IntervalQuality>
        {
            [1] = IntervalQuality.Augmented,
            [4] = IntervalQuality.Augmented,
            [5] = IntervalQuality.Diminished
        };

        private static readonly int[] IntervalNumbersOffsets = new[] { 1, 2, 2, 3, 3, 4, 5, 5, 6, 6, 7, 7 };

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
        /// Returns collection of definitions of the current <see cref="Interval"/>.
        /// </summary>
        /// <returns>Collection of definitions of the current <see cref="Interval"/>.</returns>
        public IReadOnlyCollection<IntervalDefinition> GetIntervalDefinitions()
        {
            if (_intervalDefinitions != null)
                return _intervalDefinitions;

            var result = new List<IntervalDefinition>();

            var quality = QualitiesPattern[Size % Octave.OctaveSize];
            var number = 7 * (Size / Octave.OctaveSize) + IntervalNumbersOffsets[Size % Octave.OctaveSize];

            if (quality != null)
            {
                result.Add(new IntervalDefinition(number, quality.Value));

                var additionalQuality = IntervalQuality.Augmented;

                switch (quality.Value)
                {
                    case IntervalQuality.Perfect:
                        if (number == 1)
                            additionalQuality = IntervalQuality.Diminished;
                        else
                            additionalQuality = AdditionalQualitiesPattern[number % 7];

                        if (number % 7 == 1)
                        {
                            if (number > 1)
                                result.Add(new IntervalDefinition(number - 1, IntervalQuality.Augmented));

                            result.Add(new IntervalDefinition(number + 1, IntervalQuality.Diminished));
                            return _intervalDefinitions = new ReadOnlyCollection<IntervalDefinition>(result);
                        }

                        break;
                    case IntervalQuality.Minor:
                        additionalQuality = IntervalQuality.Augmented;
                        break;
                    case IntervalQuality.Major:
                        additionalQuality = IntervalQuality.Diminished;
                        break;
                }

                switch (additionalQuality)
                {
                    case IntervalQuality.Augmented:
                        result.Add(new IntervalDefinition(number - 1, IntervalQuality.Augmented));
                        break;
                    case IntervalQuality.Diminished:
                        result.Add(new IntervalDefinition(number + 1, IntervalQuality.Diminished));
                        break;
                }
            }
            else
            {
                result.Add(new IntervalDefinition(number, IntervalQuality.Diminished));
                result.Add(new IntervalDefinition(number - 1, IntervalQuality.Augmented));
            }

            return _intervalDefinitions = new ReadOnlyCollection<IntervalDefinition>(result);
        }

        /// <summary>
        /// Gets a value indicating whether the specified interval number (1 and greater) is perfect or not.
        /// </summary>
        /// <param name="intervalNumber">Interval number to determine whether it's perfect or not.</param>
        /// <returns><c>true</c> if <paramref name="intervalNumber"/> is perfect; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="intervalNumber"/> is less than 1.</exception>
        public static bool IsPerfect(int intervalNumber)
        {
            ThrowIfArgument.IsLessThan(nameof(intervalNumber), intervalNumber, 1, "Interval number is less than 1.");

            var remainder = intervalNumber % 7 - 1;
            return remainder == 0 || remainder == 3 || remainder == 4;
        }

        /// <summary>
        /// Gets a value indicating whether quality is applicable to the specified interval number (1 and greater) or not.
        /// </summary>
        /// <param name="intervalQuality">Interval quality to check whether it's applicable to
        /// <paramref name="intervalNumber"/> or not.</param>
        /// <param name="intervalNumber">Interval number to check whether <paramref name="intervalQuality"/> is
        /// applicable to it or not.</param>
        /// <returns><c>true</c> if <paramref name="intervalQuality"/> is applicable to <paramref name="intervalNumber"/>;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="intervalQuality"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="intervalNumber"/> is less than 1.</exception>
        public static bool IsQualityApplicable(IntervalQuality intervalQuality, int intervalNumber)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(intervalQuality), intervalQuality);
            ThrowIfArgument.IsLessThan(nameof(intervalNumber), intervalNumber, 1, "Interval number is less than 1.");

            switch (intervalQuality)
            {
                case IntervalQuality.Perfect:
                    return IsPerfect(intervalNumber);
                case IntervalQuality.Minor:
                case IntervalQuality.Major:
                    return !IsPerfect(intervalNumber);
                case IntervalQuality.Diminished:
                    return intervalNumber >= 2;
                case IntervalQuality.Augmented:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets an instance of the <see cref="Interval"/> by the specified interval quality and number.
        /// </summary>
        /// <param name="intervalQuality">Interval quality.</param>
        /// <param name="intervalNumber">Interval number.</param>
        /// <returns>An instance of the <see cref="Interval"/> which represents <paramref name="intervalNumber"/>
        /// along with <paramref name="intervalQuality"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="intervalQuality"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="intervalNumber"/> is less than 1.</exception>
        /// <exception cref="ArgumentException"><paramref name="intervalQuality"/> is not applicable to
        /// <paramref name="intervalNumber"/>.</exception>
        public static Interval Get(IntervalQuality intervalQuality, int intervalNumber)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(intervalQuality), intervalQuality);
            ThrowIfArgument.IsLessThan(nameof(intervalNumber), intervalNumber, 1, "Interval number is less than 1.");

            if (!IsQualityApplicable(intervalQuality, intervalNumber))
                throw new ArgumentException($"{intervalQuality} quality is not applicable to interval number of {intervalNumber}.", nameof(intervalQuality));

            var maxIntervalNumber = 8;
            if (intervalQuality == IntervalQuality.Minor || intervalQuality == IntervalQuality.Major || intervalQuality == IntervalQuality.Augmented)
                maxIntervalNumber = 7;

            var result = intervalNumber > maxIntervalNumber
                ? ((intervalNumber - 1) / 7) * Octave.OctaveSize
                : 0;

            var additionalNumber = intervalNumber;
            if (intervalNumber > maxIntervalNumber)
                additionalNumber = ((intervalNumber - 1) % 7) + 1;

            var halfTones = IntervalsHalfTones[intervalQuality];
            result += halfTones[additionalNumber];

            return FromHalfSteps(result);
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
            if (!Cache.TryGetValue(intervalSize, out intervals))
                Cache.Add(intervalSize, intervals = new Dictionary<IntervalDirection, Interval>());

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
        /// Creates an instance of the <see cref="Interval"/> from <see cref="IntervalDefinition"/>.
        /// </summary>
        /// <param name="intervalDefinition">Interval definition to create interval from.</param>
        /// <returns><see cref="Interval"/> created from <paramref name="intervalDefinition"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinition"/> is <c>null</c>.</exception>
        public static Interval FromDefinition(IntervalDefinition intervalDefinition)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return Get(intervalDefinition.Quality, intervalDefinition.Number);
        }

        /// <summary>
        /// Converts the string representation of a musical interval to its <see cref="Interval"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing an interval to convert.</param>
        /// <param name="interval">When this method returns, contains the <see cref="Interval"/>
        /// equivalent of the musical interval contained in <paramref name="input"/>, if the conversion succeeded,
        /// or <c>null</c> if the conversion failed. The conversion fails if the <paramref name="input"/> is <c>null</c> or
        /// <see cref="string.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, out Interval interval)
        {
            return ParsingUtilities.TryParse(input, IntervalParser.TryParse, out interval);
        }

        /// <summary>
        /// Converts the string representation of a musical interval to its <see cref="Scale"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing an interval to convert.</param>
        /// <returns>A <see cref="Scale"/> equivalent to the musical interval contained in <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static Interval Parse(string input)
        {
            return ParsingUtilities.Parse<Interval>(input, IntervalParser.TryParse);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Converts the value of a <see cref="Interval"/> to a <see cref="int"/>.
        /// </summary>
        /// <param name="interval"><see cref="Interval"/> object to convert to an <see cref="int"/>.</param>
        /// <returns><paramref name="interval"/> represented as <see cref="int"/>.</returns>
        public static implicit operator int(Interval interval)
        {
            return interval.HalfSteps;
        }

        /// <summary>
        /// Converts the value of a <see cref="SevenBitNumber"/> to a <see cref="Interval"/>.
        /// </summary>
        /// <param name="interval"><see cref="SevenBitNumber"/> object to convert to an <see cref="Interval"/>.</param>
        /// <returns><paramref name="interval"/> represented as <see cref="Interval"/>.</returns>
        public static implicit operator Interval(SevenBitNumber interval)
        {
            return GetUp(interval);
        }

        /// <summary>
        /// Determines if two <see cref="Interval"/> objects are equal.
        /// </summary>
        /// <param name="interval1">The first <see cref="Interval"/> to compare.</param>
        /// <param name="interval2">The second <see cref="Interval"/> to compare.</param>
        /// <returns><c>true</c> if the intervals are equal, <c>false</c> otherwise.</returns>
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
        /// <returns><c>false</c> if the intervals are equal, <c>true</c> otherwise.</returns>
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
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
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
        /// shrunk by the <paramref name="divisor"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
        public static Interval operator -(Interval interval)
        {
            ThrowIfArgument.IsNull(nameof(interval), interval);

            return interval.Down();
        }

        #endregion

        #region IComparable<Interval>

        /// <summary>
        /// Compares the current instance with another object of the same type and returns
        /// an integer that indicates whether the current instance precedes, follows, or
        /// occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns><para>A value that indicates the relative order of the objects being compared. The
        /// return value has these meanings:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Value</term>
        /// <description>Meaning</description>
        /// </listheader>
        /// <item>
        /// <term>Less than zero</term>
        /// <description>This instance precedes <paramref name="other"/> in the sort order.</description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>This instance occurs in the same position in the sort order as <paramref name="other"/>.</description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>This instance follows <paramref name="other"/> in the sort order.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(Interval other)
        {
            return HalfSteps.CompareTo(other.HalfSteps);
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
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
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
