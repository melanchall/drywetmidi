using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents metric time on an object expressed in hours, minutes and seconds.
    /// </summary>
    public sealed class MetricTime : ITime
    {
        #region Constants

        private const int MicrosecondsInMillisecond = 1000;
        private const long TicksInMicrosecond = TimeSpan.TicksPerMillisecond / MicrosecondsInMillisecond;

        #endregion

        #region Fields

        private readonly TimeSpan _timeSpan;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTime"/> with the specified
        /// number of microseconds.
        /// </summary>
        /// <param name="totalMicroseconds">Number of microseconds which represents metric time.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="totalMicroseconds"/> is negative.</exception>
        public MetricTime(long totalMicroseconds)
        {
            if (totalMicroseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(totalMicroseconds),
                                                      totalMicroseconds,
                                                      "Number of microseconds is negative.");

            _timeSpan = new TimeSpan(totalMicroseconds * TicksInMicrosecond);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTime"/> with the specified
        /// <see cref="TimeSpan"/> object.
        /// </summary>
        /// <param name="timeSpan">Time interval to initialize the <see cref="MetricTime"/>.</param>
        public MetricTime(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTime"/> with the specified
        /// numbers of hours, minutes and seconds.
        /// </summary>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="hours"/> is negative. -or-
        /// <paramref name="minutes"/> is negative. -or- <paramref name="seconds"/> is negative.</exception>
        public MetricTime(int hours, int minutes, int seconds)
            : this(hours, minutes, seconds, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTime"/> with the specified
        /// numbers of hours, minutes, seconds and milliseconds.
        /// </summary>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="hours"/> is negative. -or-
        /// <paramref name="minutes"/> is negative. -or- <paramref name="seconds"/> is negative. -or-
        /// <paramref name="milliseconds"/> is negative.</exception>
        public MetricTime(int hours, int minutes, int seconds, int milliseconds)
        {
            if (hours < 0)
                throw new ArgumentOutOfRangeException(nameof(hours), hours, "Number of hours is negative.");

            if (minutes < 0)
                throw new ArgumentOutOfRangeException(nameof(minutes), minutes, "Number of minutes is negative.");

            if (seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Number of seconds is negative.");

            if (milliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(milliseconds), milliseconds, "Number of milliseconds is negative.");

            _timeSpan = new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value of the current <see cref="MetricTime"/> expressed in microseconds.
        /// </summary>
        public long TotalMicroseconds => _timeSpan.Ticks / TicksInMicrosecond;

        /// <summary>
        /// Gets the hours component of the time represented by the current <see cref="MetricTime"/>.
        /// </summary>
        public int Hours => _timeSpan.Hours;

        /// <summary>
        /// Gets the minutes component of the time represented by the current <see cref="MetricTime"/>.
        /// </summary>
        public int Minutes => _timeSpan.Minutes;

        /// <summary>
        /// Gets the seconds component of the time represented by the current <see cref="MetricTime"/>.
        /// </summary>
        public int Seconds => _timeSpan.Seconds;

        /// <summary>
        /// Gets the milliseconds component of the time represented by the current <see cref="MetricTime"/>.
        /// </summary>
        public int Milliseconds => _timeSpan.Milliseconds;

        #endregion

        #region Methods

        /// <summary>
        /// Converts the current <see cref="MetricTime"/> object to the <see cref="TimeSpan"/>.
        /// </summary>
        /// <returns>An instance of the <see cref="TimeSpan"/> that represents current <see cref="MetricTime"/>.</returns>
        public TimeSpan ToTimeSpan()
        {
            return new TimeSpan(_timeSpan.Ticks);
        }

        /// <summary>
        /// Converts the value of the current <see cref="MetricTime"/> object to its equivalent string
        /// representation by using the specified format and culture-specific formatting
        /// information.
        /// </summary>
        /// <param name="format">A standard or custom <see cref="TimeSpan"/> format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The string representation of the current <see cref="MetricTime"/> value, as specified
        /// by <paramref name="format"/> and <paramref name="formatProvider"/>.</returns>
        /// <exception cref="FormatException"><paramref name="format"/> is not recognized or is not supported.</exception>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return _timeSpan.ToString(format, formatProvider);
        }

        /// <summary>
        /// Converts the value of the current <see cref="MetricTime"/> object to its equivalent string
        /// representation by using the specified format.
        /// </summary>
        /// <param name="format">A standard or custom <see cref="TimeSpan"/> format string.</param>
        /// <returns>The string representation of the current <see cref="MetricTime"/> value in the format
        /// specified by the format parameter.</returns>
        /// <exception cref="FormatException"><paramref name="format"/> is not recognized or is not supported.</exception>
        public string ToString(string format)
        {
            return _timeSpan.ToString(format);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="time">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(MetricTime time)
        {
            if (ReferenceEquals(null, time))
                return false;

            if (ReferenceEquals(this, time))
                return true;

            return TotalMicroseconds == time.TotalMicroseconds;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Adds two specified <see cref="MetricTime"/> instances.
        /// </summary>
        /// <param name="time1">The first time to add.</param>
        /// <param name="time2">The second time to add.</param>
        /// <returns>An object whose value is the sum of the values of <paramref name="time1"/> and
        /// <paramref name="time2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        public static MetricTime operator +(MetricTime time1, MetricTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            return new MetricTime(time1.TotalMicroseconds + time2.TotalMicroseconds);
        }

        /// <summary>
        /// Subtracts a specified <see cref="MetricTime"/> from another specified <see cref="MetricTime"/>.
        /// </summary>
        /// <param name="time1">The minuend.</param>
        /// <param name="time2">The subtrahend.</param>
        /// <returns>An object whose value is the result of the value of <paramref name="time1"/> minus
        /// the value of <paramref name="time2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="time1"/> is less than <paramref name="time2"/>.</exception>
        public static MetricTime operator -(MetricTime time1, MetricTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            if (time1 < time2)
                throw new ArgumentException("First time is less than second one.", nameof(time1));

            return new MetricTime(time1.TotalMicroseconds - time2.TotalMicroseconds);
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTime"/> is less than another specified
        /// <see cref="MetricTime"/>.
        /// </summary>
        /// <param name="time1">The first time to compare.</param>
        /// <param name="time2">The second time to compare.</param>
        /// <returns>true if the value of <paramref name="time1"/> is less than the value of <paramref name="time2"/>;
        /// otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        public static bool operator <(MetricTime time1, MetricTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            return time1.TotalMicroseconds < time2.TotalMicroseconds;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTime"/> is greater than another specified
        /// <see cref="MetricTime"/>.
        /// </summary>
        /// <param name="time1">The first time to compare.</param>
        /// <param name="time2">The second time to compare.</param>
        /// <returns>true if the value of <paramref name="time1"/> is greater than the value of <paramref name="time2"/>;
        /// otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        public static bool operator >(MetricTime time1, MetricTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            return time1.TotalMicroseconds > time2.TotalMicroseconds;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTime"/> is less than or equal to another specified
        /// <see cref="MetricTime"/>.
        /// </summary>
        /// <param name="time1">The first time to compare.</param>
        /// <param name="time2">The second time to compare.</param>
        /// <returns>true if the value of <paramref name="time1"/> is less than or equal to the value of
        /// <paramref name="time2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        public static bool operator <=(MetricTime time1, MetricTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            return time1.TotalMicroseconds <= time2.TotalMicroseconds;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTime"/> is greater than or equal to another specified
        /// <see cref="MetricTime"/>.
        /// </summary>
        /// <param name="time1">The first time to compare.</param>
        /// <param name="time2">The second time to compare.</param>
        /// <returns>true if the value of <paramref name="time1"/> is greater than or equal to the value of
        /// <paramref name="time2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time1"/> is null. -or-
        /// <paramref name="time2"/> is null.</exception>
        public static bool operator >=(MetricTime time1, MetricTime time2)
        {
            if (time1 == null)
                throw new ArgumentNullException(nameof(time1));

            if (time2 == null)
                throw new ArgumentNullException(nameof(time2));

            return time1.TotalMicroseconds >= time2.TotalMicroseconds;
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
            return Equals(obj as MetricTime);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return TotalMicroseconds.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return _timeSpan.ToString();
        }

        #endregion
    }
}
