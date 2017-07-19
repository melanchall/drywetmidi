using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents musical time on an object expressed in bars, beats and ticks.
    /// </summary>
    public sealed class MusicalTime : ITime
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicalTime"/>.
        /// </summary>
        public MusicalTime()
            : this(0, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicalTime"/> with the specified
        /// numbers of bars, beats.
        /// </summary>
        /// <param name="bars">Number of bars.</param>
        /// <param name="beats">Number of beats.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bars"/> is negative. -or-
        /// <paramref name="beats"/> is negative.</exception>
        public MusicalTime(int bars, int beats)
            : this(bars, beats, Fraction.NoFraction)
        {
        }

        public MusicalTime(Fraction fraction)
            : this(0, 0, fraction)
        {
        }

        public MusicalTime(MusicalFraction fraction, int fractionCount)
        {
            if (fraction == null)
                throw new ArgumentNullException(nameof(fraction));

            if (fractionCount < 0)
                throw new ArgumentOutOfRangeException(nameof(fractionCount), fractionCount, "Fraction count is negative.");

            Fraction = new[] { new MusicalFractionCount(fraction, fractionCount) }.ToMathFraction();
        }

        public MusicalTime(params MusicalFraction[] fractions)
            : this(fractions as IEnumerable<MusicalFraction>)
        {
        }

        public MusicalTime(IEnumerable<MusicalFraction> fractions)
            : this(fractions?.Select(f => new MusicalFractionCount(f, 1)))
        {
        }

        public MusicalTime(params MusicalFractionCount[] fractionsCounts)
            : this(fractionsCounts as IEnumerable<MusicalFractionCount>)
        {
        }

        public MusicalTime(IEnumerable<MusicalFractionCount> fractionsCounts)
        {
            if (fractionsCounts == null)
                throw new ArgumentNullException(nameof(fractionsCounts));

            Fraction = fractionsCounts.ToMathFraction();
        }

        public MusicalTime(int bars, int beats, Fraction fraction)
        {
            if (bars < 0)
                throw new ArgumentOutOfRangeException("Number of bars is negative.", bars, nameof(bars));

            if (beats < 0)
                throw new ArgumentOutOfRangeException("Number of beats is negative.", beats, nameof(beats));

            if (fraction == null)
                throw new ArgumentNullException(nameof(fraction));

            Bars = bars;
            Beats = beats;
            Fraction = fraction;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the bars component of the time represented by the current <see cref="MusicalTime"/>.
        /// </summary>
        public int Bars { get; }

        /// <summary>
        /// Gets the beats component of the time represented by the current <see cref="MusicalTime"/>.
        /// </summary>
        public int Beats { get; }

        public Fraction Fraction { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="time">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(MusicalTime time)
        {
            if (ReferenceEquals(null, time))
                return false;

            if (ReferenceEquals(this, time))
                return true;

            return Bars == time.Bars &&
                   Beats == time.Beats &&
                   Fraction == time.Fraction;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Adds two specified <see cref="MusicalTime"/> instances.
        /// </summary>
        /// <param name="time1">The first time to add.</param>
        /// <param name="time2">The second time to add.</param>
        /// <returns>An object whose value is the sum of the values of <paramref name="time1"/> and
        /// <paramref name="time2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        public static MusicalTime operator +(MusicalTime time1, MusicalTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            return new MusicalTime(time1.Bars + time2.Bars,
                                   time1.Beats + time2.Beats,
                                   time1.Fraction + time2.Fraction);
        }

        public static MusicalTime operator +(MusicalTime time, MusicalLength length)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (length == null)
                throw new ArgumentNullException(nameof(length));

            return new MusicalTime(time.Bars,
                                   time.Beats,
                                   time.Fraction + length.Fraction);
        }

        /// <summary>
        /// Subtracts a specified <see cref="MusicalTime"/> from another specified <see cref="MusicalTime"/>.
        /// </summary>
        /// <param name="time1">The minuend.</param>
        /// <param name="time2">The subtrahend.</param>
        /// <returns>An object whose value is the result of the value of <paramref name="time1"/> minus
        /// the value of <paramref name="time2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="time1"/> is less than <paramref name="time2"/>.</exception>
        public static MusicalTime operator -(MusicalTime time1, MusicalTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            if (time1 < time2)
                throw new ArgumentException("First time is less than second one.", nameof(time1));
            
            return new MusicalTime(time1.Bars - time2.Bars,
                                   time1.Beats - time2.Beats,
                                   time1.Fraction - time2.Fraction);
        }

        public static MusicalTime operator -(MusicalTime time, MusicalLength length)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (length == null)
                throw new ArgumentNullException(nameof(length));

            if (time.Fraction < length.Fraction)
                throw new ArgumentException("First fraction is less than second one.", nameof(time));

            return new MusicalTime(time.Bars,
                                   time.Beats,
                                   time.Fraction - length.Fraction);
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MusicalTime"/> is less than another specified
        /// <see cref="MusicalTime"/>.
        /// </summary>
        /// <param name="time1">The first time to compare.</param>
        /// <param name="time2">The second time to compare.</param>
        /// <returns>true if the value of <paramref name="time1"/> is less than the value of <paramref name="time2"/>;
        /// otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        public static bool operator <(MusicalTime time1, MusicalTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            return time1.Bars < time2.Bars ||
                   (time1.Bars == time2.Bars && (time1.Beats < time2.Beats ||
                                                 (time1.Beats == time2.Beats && time1.Fraction < time2.Fraction)));
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MusicalTime"/> is greater than another specified
        /// <see cref="MusicalTime"/>.
        /// </summary>
        /// <param name="time1">The first time to compare.</param>
        /// <param name="time2">The second time to compare.</param>
        /// <returns>true if the value of <paramref name="time1"/> is greater than the value of <paramref name="time2"/>;
        /// otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        public static bool operator >(MusicalTime time1, MusicalTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            return time1.Bars > time2.Bars ||
                   (time1.Bars == time2.Bars && (time1.Beats > time2.Beats ||
                                                 (time1.Beats == time2.Beats && time1.Fraction > time2.Fraction)));
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MusicalTime"/> is less than or equal to another specified
        /// <see cref="MusicalTime"/>.
        /// </summary>
        /// <param name="time1">The first time to compare.</param>
        /// <param name="time2">The second time to compare.</param>
        /// <returns>true if the value of <paramref name="time1"/> is less than or equal to the value of
        /// <paramref name="time2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        public static bool operator <=(MusicalTime time1, MusicalTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            return time1.Bars <= time2.Bars ||
                   (time1.Bars == time2.Bars && (time1.Beats <= time2.Beats ||
                                                 (time1.Beats == time2.Beats && time1.Fraction <= time2.Fraction)));
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MusicalTime"/> is greater than or equal to another specified
        /// <see cref="MusicalTime"/>.
        /// </summary>
        /// <param name="time1">The first time to compare.</param>
        /// <param name="time2">The second time to compare.</param>
        /// <returns>true if the value of <paramref name="time1"/> is greater than or equal to the value of
        /// <paramref name="time2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        public static bool operator >=(MusicalTime time1, MusicalTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            return time1.Bars >= time2.Bars ||
                   (time1.Bars == time2.Bars && (time1.Beats >= time2.Beats ||
                                                 (time1.Beats == time2.Beats && time1.Fraction >= time2.Fraction)));
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
            return Equals(obj as MusicalTime);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Bars.GetHashCode() ^
                   Beats.GetHashCode() ^
                   Fraction.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Bars}:{Beats}:{Fraction}";
        }

        #endregion
    }
}
