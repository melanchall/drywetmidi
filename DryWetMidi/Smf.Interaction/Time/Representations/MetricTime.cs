using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents metric time of an object expressed in hours, minutes and seconds.
    /// </summary>
    public sealed class MetricTime : ITime, IFormattable
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
        /// Initializes a new instance of the <see cref="MetricTime"/>.
        /// </summary>
        public MetricTime()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTime"/> with the specified
        /// number of microseconds.
        /// </summary>
        /// <param name="totalMicroseconds">Number of microseconds which represents metric time.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="totalMicroseconds"/> is negative.</exception>
        public MetricTime(long totalMicroseconds)
        {
            ThrowIfArgument.IsNegative(nameof(totalMicroseconds),
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
            ThrowIfArgument.IsNegative(nameof(hours), hours, "Number of hours is negative.");
            ThrowIfArgument.IsNegative(nameof(minutes), minutes, "Number of minutes is negative.");
            ThrowIfArgument.IsNegative(nameof(seconds), seconds, "Number of seconds is negative.");
            ThrowIfArgument.IsNegative(nameof(milliseconds), milliseconds, "Number of milliseconds is negative.");

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

        public static bool TryParse(string input, out MetricTime time)
        {
            return MetricTimeParser.TryParse(input, out time) == MetricTimeParser.ParsingResult.Parsed;
        }

        public static MetricTime Parse(string input)
        {
            var parsingResult = MetricTimeParser.TryParse(input, out var fraction);
            if (parsingResult == MetricTimeParser.ParsingResult.Parsed)
                return fraction;

            throw MetricTimeParser.GetException(parsingResult, nameof(input));
        }

        #endregion

        #region Operators

        /// <summary>
        /// Casts <see cref="TimeSpan"/> to <see cref="MetricTime"/>.
        /// </summary>
        /// <param name="timeSpan"><see cref="TimeSpan"/> to cast to <see cref="MetricTime"/>.</param>
        public static implicit operator MetricTime(TimeSpan timeSpan)
        {
            return new MetricTime(timeSpan);
        }

        /// <summary>
        /// Casts <see cref="MetricTime"/> to <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="time"><see cref="MetricTime"/> to cast to <see cref="TimeSpan"/>.</param>
        public static implicit operator TimeSpan(MetricTime time)
        {
            return time._timeSpan;
        }

        /// <summary>
        /// Determines if two <see cref="MetricTime"/> objects are equal.
        /// </summary>
        /// <param name="time1">The first <see cref="MetricTime"/> to compare.</param>
        /// <param name="time2">The second <see cref="MetricTime"/> to compare.</param>
        /// <returns>true if the times are equal, false otherwise.</returns>
        public static bool operator ==(MetricTime time1, MetricTime time2)
        {
            if (ReferenceEquals(time1, time2))
                return true;

            if (ReferenceEquals(null, time1) || ReferenceEquals(null, time2))
                return false;

            return time1.TotalMicroseconds == time2.TotalMicroseconds;
        }

        /// <summary>
        /// Determines if two <see cref="MetricTime"/> objects are not equal.
        /// </summary>
        /// <param name="time1">The first <see cref="MetricTime"/> to compare.</param>
        /// <param name="time2">The second <see cref="MetricTime"/> to compare.</param>
        /// <returns>false if the times are equal, true otherwise.</returns>
        public static bool operator !=(MetricTime time1, MetricTime time2)
        {
            return !(time1 == time2);
        }

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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

            return new MetricTime(time1.TotalMicroseconds + time2.TotalMicroseconds);
        }

        /// <summary>
        /// Sums <see cref="MetricTime"/> and <see cref="MetricLength"/>.
        /// </summary>
        /// <param name="time">The <see cref="MetricTime"/> to add.</param>
        /// <param name="length">The <see cref="MetricLength"/> to add.</param>
        /// <returns>The sum of <paramref name="time"/> and <paramref name="length"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        public static MetricTime operator +(MetricTime time, MetricLength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

            return new MetricTime(time.TotalMicroseconds + length.TotalMicroseconds);
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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

            if (time1 < time2)
                throw new ArgumentException("First time is less than second one.", nameof(time1));

            return new MetricTime(time1.TotalMicroseconds - time2.TotalMicroseconds);
        }

        /// <summary>
        /// Subtracts <see cref="MetricLength"/> from <see cref="MetricTime"/>.
        /// </summary>
        /// <param name="time">The minuend.</param>
        /// <param name="length">The subtrahend.</param>
        /// <returns>The result of subtracting <paramref name="length"/> from <paramref name="time"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null. -or-
        /// <paramref name="length"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="time"/> is less than <paramref name="length"/>.</exception>
        public static MetricTime operator -(MetricTime time, MetricLength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

            if (time.TotalMicroseconds < length.TotalMicroseconds)
                throw new ArgumentException("Time is less than length.", nameof(time));

            return new MetricTime(time.TotalMicroseconds - length.TotalMicroseconds);
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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

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
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

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
            return this == (obj as MetricTime);
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
            return $"{Hours}:{Minutes}:{Seconds}:{Milliseconds}";
        }

        #endregion

        #region IFormattable

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

        #endregion
    }
}
