using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a musical interval definition in terms of half steps number.
    /// </summary>
    public sealed class IntervalDefinition
    {
        #region Fields

        private static readonly Dictionary<SevenBitNumber, Dictionary<IntervalDirection, IntervalDefinition>> _cache =
            new Dictionary<SevenBitNumber, Dictionary<IntervalDirection, IntervalDefinition>>();

        #endregion

        #region Constants

        /// <summary>
        /// Interval of zero half steps up.
        /// </summary>
        public static readonly IntervalDefinition Zero = FromHalfSteps(0);

        /// <summary>
        /// Interval of one half step up.
        /// </summary>
        public static readonly IntervalDefinition One = FromHalfSteps(1);

        /// <summary>
        /// Interval of two half steps up.
        /// </summary>
        public static readonly IntervalDefinition Two = FromHalfSteps(2);

        /// <summary>
        /// Interval of three half steps up.
        /// </summary>
        public static readonly IntervalDefinition Three = FromHalfSteps(3);

        /// <summary>
        /// Interval of four half steps up.
        /// </summary>
        public static readonly IntervalDefinition Four = FromHalfSteps(4);

        /// <summary>
        /// Interval of five half steps up.
        /// </summary>
        public static readonly IntervalDefinition Five = FromHalfSteps(5);

        /// <summary>
        /// Interval of six half steps up.
        /// </summary>
        public static readonly IntervalDefinition Six = FromHalfSteps(6);

        /// <summary>
        /// Interval of seven half steps up.
        /// </summary>
        public static readonly IntervalDefinition Seven = FromHalfSteps(7);

        /// <summary>
        /// Interval of eight half steps up.
        /// </summary>
        public static readonly IntervalDefinition Eight = FromHalfSteps(8);

        /// <summary>
        /// Interval of nine half steps up.
        /// </summary>
        public static readonly IntervalDefinition Nine = FromHalfSteps(9);

        /// <summary>
        /// Interval of ten half steps up.
        /// </summary>
        public static readonly IntervalDefinition Ten = FromHalfSteps(10);

        /// <summary>
        /// Interval of eleven half steps up.
        /// </summary>
        public static readonly IntervalDefinition Eleven = FromHalfSteps(11);

        /// <summary>
        /// Interval of twelve half steps up (one octave up).
        /// </summary>
        public static readonly IntervalDefinition Twelve = FromHalfSteps(12);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalDefinition"/> with the
        /// specified interval and its direction.
        /// </summary>
        /// <param name="interval">The interval as a number of half steps away.</param>
        /// <param name="direction">The direction of an interval (up or down).</param>
        private IntervalDefinition(SevenBitNumber interval, IntervalDirection direction)
        {
            Interval = interval;
            Direction = direction;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the interval represented by the current <see cref="IntervalDefinition"/> as a
        /// number of half steps away.
        /// </summary>
        public SevenBitNumber Interval { get; }

        /// <summary>
        /// Gets the direction of the interval represented by the current <see cref="IntervalDefinition"/>.
        /// </summary>
        public IntervalDirection Direction { get; }

        /// <summary>
        /// Gets signed half steps number which represents an interval of the current <see cref="IntervalDefinition"/>.
        /// </summary>
        public int HalfSteps => Direction == IntervalDirection.Up
            ? Interval
            : -Interval;

        #endregion

        #region Methods

        /// <summary>
        /// Returns upward version of the current <see cref="IntervalDefinition"/>.
        /// </summary>
        /// <returns>An upward version of the current <see cref="IntervalDefinition"/>.</returns>
        public IntervalDefinition Up()
        {
            return Get(Interval, IntervalDirection.Up);
        }

        /// <summary>
        /// Returns downward version of the current <see cref="IntervalDefinition"/>.
        /// </summary>
        /// <returns>A downward version of the current <see cref="IntervalDefinition"/>.</returns>
        public IntervalDefinition Down()
        {
            return Get(Interval, IntervalDirection.Down);
        }

        /// <summary>
        /// Returns an <see cref="IntervalDefinition"/> by the specified half steps number and
        /// interval's direction.
        /// </summary>
        /// <param name="interval">The interval as a number of half steps away.</param>
        /// <param name="direction">The direction of an interval (up or down).</param>
        /// <returns>An <see cref="IntervalDefinition"/> with the specified interval and direction.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="direction"/> specified an
        /// invalid value.</exception>
        public static IntervalDefinition Get(SevenBitNumber interval, IntervalDirection direction)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(direction), direction);

            if (!_cache.TryGetValue(interval, out Dictionary<IntervalDirection, IntervalDefinition> intervalDefinitions))
                _cache.Add(interval, intervalDefinitions = new Dictionary<IntervalDirection, IntervalDefinition>());

            if (!intervalDefinitions.TryGetValue(direction, out IntervalDefinition intervalDefinition))
                intervalDefinitions.Add(direction, intervalDefinition = new IntervalDefinition(interval, direction));

            return intervalDefinition;
        }

        /// <summary>
        /// Returns an upward <see cref="IntervalDefinition"/> by the specified half steps number.
        /// </summary>
        /// <param name="interval">The interval as a number of half steps away.</param>
        /// <returns>An upward <see cref="IntervalDefinition"/> with the specified interval.</returns>
        public static IntervalDefinition GetUp(SevenBitNumber interval)
        {
            return Get(interval, IntervalDirection.Up);
        }

        /// <summary>
        /// Returns a downward <see cref="IntervalDefinition"/> by the specified half steps number.
        /// </summary>
        /// <param name="interval">The interval as a number of half steps away.</param>
        /// <returns>A downward <see cref="IntervalDefinition"/> with the specified interval.</returns>
        public static IntervalDefinition GetDown(SevenBitNumber interval)
        {
            return Get(interval, IntervalDirection.Down);
        }

        /// <summary>
        /// Returns an <see cref="IntervalDefinition"/> by the specified signed number of
        /// half steps where negative one means downward interval.
        /// </summary>
        /// <param name="halfSteps">The number of half steps.</param>
        /// <returns>An <see cref="IntervalDefinition"/> represented by the <paramref name="halfSteps"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfSteps"/> is out of range
        /// (result interval is out of the [-127,127] range).</exception>
        public static IntervalDefinition FromHalfSteps(int halfSteps)
        {
            ThrowIfArgument.IsOutOfRange(nameof(halfSteps),
                                         halfSteps,
                                         -SevenBitNumber.MaxValue,
                                         SevenBitNumber.MaxValue,
                                         "Half steps number is out of range.");

            return Get((SevenBitNumber)Math.Abs(halfSteps),
                       Math.Sign(halfSteps) < 0 ? IntervalDirection.Down : IntervalDirection.Up);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Converts the value of a <see cref="IntervalDefinition"/> to a <see cref="int"/>.
        /// </summary>
        /// <param name="intervalDefinition"><see cref="IntervalDefinition"/> object to convert to an <see cref="int"/>.</param>
        public static implicit operator int(IntervalDefinition intervalDefinition)
        {
            return intervalDefinition.HalfSteps;
        }

        /// <summary>
        /// Converts the value of a <see cref="SevenBitNumber"/> to a <see cref="IntervalDefinition"/>.
        /// </summary>
        /// <param name="interval"><see cref="SevenBitNumber"/> object to convert to an <see cref="IntervalDefinition"/>.</param>
        public static implicit operator IntervalDefinition(SevenBitNumber interval)
        {
            return GetUp(interval);
        }

        /// <summary>
        /// Determines if two <see cref="IntervalDefinition"/> objects are equal.
        /// </summary>
        /// <param name="intervalDefinition1">The first <see cref="IntervalDefinition"/> to compare.</param>
        /// <param name="intervalDefinition2">The second <see cref="IntervalDefinition"/> to compare.</param>
        /// <returns>true if the interval definitions are equal, false otherwise.</returns>
        public static bool operator ==(IntervalDefinition intervalDefinition1, IntervalDefinition intervalDefinition2)
        {
            if (ReferenceEquals(intervalDefinition1, intervalDefinition2))
                return true;

            if (ReferenceEquals(null, intervalDefinition1) || ReferenceEquals(null, intervalDefinition2))
                return false;

            return intervalDefinition1.HalfSteps == intervalDefinition2.HalfSteps;
        }

        /// <summary>
        /// Determines if two <see cref="IntervalDefinition"/> objects are not equal.
        /// </summary>
        /// <param name="intervalDefinition1">The first <see cref="IntervalDefinition"/> to compare.</param>
        /// <param name="intervalDefinition2">The second <see cref="IntervalDefinition"/> to compare.</param>
        /// <returns>false if the interval definitions are equal, true otherwise.</returns>
        public static bool operator !=(IntervalDefinition intervalDefinition1, IntervalDefinition intervalDefinition2)
        {
            return !(intervalDefinition1 == intervalDefinition2);
        }

        /// <summary>
        /// Adds the specified number of half steps to an <see cref="IntervalDefinition"/>.
        /// </summary>
        /// <param name="intervalDefinition">The interval definition to add half steps to.</param>
        /// <param name="halfSteps">The number of half steps to add to the <paramref name="intervalDefinition"/>.</param>
        /// <returns>The <see cref="IntervalDefinition"/> which is the <paramref name="intervalDefinition"/>
        /// shifted by the <paramref name="halfSteps"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinition"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfSteps"/> is out of range
        /// (result interval is out of the [-127,127] range).</exception>
        public static IntervalDefinition operator +(IntervalDefinition intervalDefinition, int halfSteps)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return FromHalfSteps(intervalDefinition.HalfSteps + halfSteps);
        }

        /// <summary>
        /// Subtracts the specified number of half steps from an <see cref="IntervalDefinition"/>.
        /// </summary>
        /// <param name="intervalDefinition">The interval definition to subtract half steps from.</param>
        /// <param name="halfSteps">The number of half steps to subtract from the <paramref name="intervalDefinition"/>.</param>
        /// <returns>The <see cref="IntervalDefinition"/> which is the <paramref name="intervalDefinition"/>
        /// shifted by the <paramref name="halfSteps"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinition"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfSteps"/> is out of range
        /// (result interval is out of the [-127,127] range).</exception>
        public static IntervalDefinition operator -(IntervalDefinition intervalDefinition, int halfSteps)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return FromHalfSteps(intervalDefinition.HalfSteps - halfSteps);
        }

        /// <summary>
        /// Stretches the specified <see cref="IntervalDefinition"/> by multiplying it by an integer number.
        /// </summary>
        /// <param name="intervalDefinition">The interval definition to stretch.</param>
        /// <param name="multiplier">The number to multiply the <paramref name="intervalDefinition"/> by.</param>
        /// <returns>The <see cref="IntervalDefinition"/> which is the <paramref name="intervalDefinition"/>
        /// stretched by the <paramref name="multiplier"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinition"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="multiplier"/> is out of range
        /// (result interval is out of the [-127,127] range).</exception>
        public static IntervalDefinition operator *(IntervalDefinition intervalDefinition, int multiplier)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return FromHalfSteps(intervalDefinition.HalfSteps * multiplier);
        }

        /// <summary>
        /// Shrinks the specified <see cref="IntervalDefinition"/> by dividing it by an integer number.
        /// </summary>
        /// <param name="intervalDefinition">The interval definition to shrink.</param>
        /// <param name="divisor">The number to divide the <paramref name="intervalDefinition"/> by.</param>
        /// <returns>The <see cref="IntervalDefinition"/> which is the <paramref name="intervalDefinition"/>
        /// shrinked by the <paramref name="divisor"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinition"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is zero.</exception>
        public static IntervalDefinition operator /(IntervalDefinition intervalDefinition, int divisor)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            if (divisor == 0)
                throw new ArgumentOutOfRangeException(nameof(divisor), divisor, "Divisor is zero.");

            return FromHalfSteps(intervalDefinition.HalfSteps / divisor);
        }

        /// <summary>
        /// Returns upward version of the specified <see cref="IntervalDefinition"/>.
        /// </summary>
        /// <param name="intervalDefinition">The <see cref="IntervalDefinition"/> to get upward version of.</param>
        /// <returns>An upward version of the <paramref name="intervalDefinition"/>.</returns>
        /// <remarks>
        /// This operator produces the same result as the <see cref="Up"/> method.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinition"/> is null.</exception>
        public static IntervalDefinition operator +(IntervalDefinition intervalDefinition)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return intervalDefinition.Up();
        }

        /// <summary>
        /// Returns downward version of the specified <see cref="IntervalDefinition"/>.
        /// </summary>
        /// <param name="intervalDefinition">The <see cref="IntervalDefinition"/> to get downward version of.</param>
        /// <returns>A downward version of the <paramref name="intervalDefinition"/>.</returns>
        /// <remarks>
        /// This operator produces the same result as the <see cref="Down"/> method.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="intervalDefinition"/> is null.</exception>
        public static IntervalDefinition operator -(IntervalDefinition intervalDefinition)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return intervalDefinition.Down();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{(Direction == IntervalDirection.Up ? "+" : "-")}{Interval}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as IntervalDefinition);
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
