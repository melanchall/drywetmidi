using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents musical time on an object expressed in bars, beats and ticks.
    /// </summary>
    public sealed class MusicalTime : ITime
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicalTime"/> with the specified
        /// numbers of bars, beats and ticks and the specified beat length.
        /// </summary>
        /// <param name="bars">Number of bars.</param>
        /// <param name="beats">Number of beats.</param>
        /// <param name="ticks">Number of ticks.</param>
        /// <param name="beatLength">Length of a beat in ticks.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bars"/> is negative. -or-
        /// <paramref name="beats"/> is negative. -or- <paramref name="ticks"/> is negative. -or-
        /// <paramref name="beatLength"/> is zero or negative.</exception>
        public MusicalTime(int bars, int beats, int ticks, int beatLength)
        {
            if (bars < 0)
                throw new ArgumentOutOfRangeException("Number of bars is negative.", bars, nameof(bars));

            if (beats < 0)
                throw new ArgumentOutOfRangeException("Number of beats is negative.", beats, nameof(beats));

            if (ticks < 0)
                throw new ArgumentOutOfRangeException("Number of ticks is negative.", ticks, nameof(ticks));

            if (beatLength <= 0)
                throw new ArgumentOutOfRangeException("Beat length is zero or negative.", beatLength, nameof(beatLength));

            Bars = bars;
            Beats = beats + ticks / beatLength;
            Ticks = ticks % beatLength;
            BeatLength = beatLength;
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
        /// Gets the ticks component of the time represented by the current <see cref="MusicalTime"/>.
        /// </summary>
        public int Ticks { get; }

        /// <summary>
        /// Gets length of a beat of the time represented by the current <see cref="MusicalTime"/>.
        /// </summary>
        public int BeatLength { get; }

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
                   Ticks == time.Ticks &&
                   BeatLength == time.BeatLength;
        }

        private static void EqualizeTicks(MusicalTime time1, MusicalTime time2, out int beatLength, out int ticks1, out int ticks2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            beatLength = NumberUtilities.LeastCommonMultiple(time1.BeatLength, time2.BeatLength);
            ticks1 = beatLength * time1.Ticks / time1.BeatLength;
            ticks2 = beatLength * time2.Ticks / time2.BeatLength;
        }

        private static void GetPartsDifferencies(MusicalTime time1, MusicalTime time2, out int barsDifference, out int beatsDifference, out int ticksDifference)
        {
            barsDifference = time1.Bars - time2.Bars;
            beatsDifference = time1.Beats - time2.Beats;

            int beatLength, ticks1, ticks2;
            EqualizeTicks(time1, time2, out beatLength, out ticks1, out ticks2);
            ticksDifference = ticks1 - ticks2;
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

            int beatLength, ticks1, ticks2;
            EqualizeTicks(time1, time2, out beatLength, out ticks1, out ticks2);

            return new MusicalTime(time1.Bars + time2.Bars,
                                   time1.Beats + time2.Beats,
                                   ticks1 + ticks2,
                                   beatLength);
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

            int beatLength, ticks1, ticks2;
            EqualizeTicks(time1, time2, out beatLength, out ticks1, out ticks2);

            return new MusicalTime(time1.Bars - time2.Bars,
                                   time1.Beats - time2.Beats,
                                   ticks1 - ticks2,
                                   beatLength);
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

            int barsDifference, beatsDifference, ticksDifference;
            GetPartsDifferencies(time1, time2, out barsDifference, out beatsDifference, out ticksDifference);

            return barsDifference < 0 ||
                   (barsDifference == 0 && (beatsDifference < 0 ||
                                            (beatsDifference == 0 && ticksDifference < 0)));
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

            int barsDifference, beatsDifference, ticksDifference;
            GetPartsDifferencies(time1, time2, out barsDifference, out beatsDifference, out ticksDifference);

            return barsDifference > 0 ||
                   (barsDifference == 0 && (beatsDifference > 0 ||
                                            (beatsDifference == 0 && ticksDifference > 0)));
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

            int barsDifference, beatsDifference, ticksDifference;
            GetPartsDifferencies(time1, time2, out barsDifference, out beatsDifference, out ticksDifference);

            return barsDifference < 0 ||
                   (barsDifference == 0 && (beatsDifference < 0 ||
                                            (beatsDifference == 0 && ticksDifference <= 0)));
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

            int barsDifference, beatsDifference, ticksDifference;
            GetPartsDifferencies(time1, time2, out barsDifference, out beatsDifference, out ticksDifference);

            return barsDifference > 0 ||
                   (barsDifference == 0 && (beatsDifference > 0 ||
                                            (beatsDifference == 0 && ticksDifference >= 0)));
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
                   Ticks.GetHashCode() ^
                   BeatLength.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Bars}:{Beats}[{BeatLength}]:{Ticks}";
        }

        #endregion
    }
}
