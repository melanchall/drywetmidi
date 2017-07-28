using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents musical time of an object expressed in bars, beats and fraction of the
    /// whole note length.
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
        /// number of bars and beats.
        /// </summary>
        /// <param name="bars">Number of bars.</param>
        /// <param name="beats">Number of beats.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bars"/> is negative. -or-
        /// <paramref name="beats"/> is negative.</exception>
        public MusicalTime(int bars, int beats)
            : this(bars, beats, Fraction.ZeroFraction)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicalTime"/> with the specified
        /// fraction of the whole note length.
        /// </summary>
        /// <param name="fraction">Fraction of the whole note length.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fraction"/> is null.</exception>
        public MusicalTime(Fraction fraction)
            : this(0, 0, fraction)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicalTime"/> with the specified
        /// number of bars, beats and fraction of the whole note length.
        /// </summary>
        /// <param name="bars">Number of bars.</param>
        /// <param name="beats">Number of beats.</param>
        /// <param name="fraction">Fraction of the whole note length.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bars"/> is negative. -or-
        /// <paramref name="beats"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="fraction"/> is null.</exception>
        public MusicalTime(int bars, int beats, Fraction fraction)
        {
            ThrowIfArgument.IsNegative(nameof(bars), bars, "Number of bars is negative.");
            ThrowIfArgument.IsNegative(nameof(beats), beats, "Number of beats is negative.");
            ThrowIfArgument.IsNull(nameof(fraction), fraction);

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

        /// <summary>
        /// Gets the fraction component of the time represented by the current <see cref="MusicalTime"/>.
        /// </summary>
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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

            return new MusicalTime(time1.Bars + time2.Bars,
                                   time1.Beats + time2.Beats,
                                   time1.Fraction + time2.Fraction);
        }

        /// <summary>
        /// Sums <see cref="MusicalTime"/> and <see cref="MusicalLength"/>.
        /// </summary>
        /// <param name="time">The <see cref="MusicalTime"/> to add.</param>
        /// <param name="length">The <see cref="MusicalLength"/> to add.</param>
        /// <returns>The sum of <paramref name="time"/> and <paramref name="length"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        public static MusicalTime operator +(MusicalTime time, MusicalLength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

            if (time1 < time2)
                throw new ArgumentException("First time is less than second one.", nameof(time1));
            
            return new MusicalTime(time1.Bars - time2.Bars,
                                   time1.Beats - time2.Beats,
                                   time1.Fraction - time2.Fraction);
        }

        /// <summary>
        /// Subtracts <see cref="MusicalLength"/> from <see cref="MusicalTime"/>.
        /// </summary>
        /// <param name="time">The minuend.</param>
        /// <param name="length">The subtrahend.</param>
        /// <returns>The result of subtracting <paramref name="length"/> from <paramref name="time"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="time"/> is less than <paramref name="length"/>.</exception>
        public static MusicalTime operator -(MusicalTime time, MusicalLength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

            if (time.Fraction < length.Fraction)
                throw new ArgumentException("Time's fraction is less than length's fraction.", nameof(time));

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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

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
