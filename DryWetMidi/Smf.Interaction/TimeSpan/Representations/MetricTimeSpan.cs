using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents metric time span which represents hours, minutes and seconds.
    /// </summary>
    public sealed class MetricTimeSpan : ITimeSpan
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
        /// Initializes a new instance of the <see cref="MetricTimeSpan"/>.
        /// </summary>
        public MetricTimeSpan()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTimeSpan"/> with the specified
        /// number of microseconds.
        /// </summary>
        /// <param name="totalMicroseconds">Number of microseconds which represents metric time span.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="totalMicroseconds"/> is negative.</exception>
        public MetricTimeSpan(long totalMicroseconds)
        {
            ThrowIfArgument.IsNegative(nameof(totalMicroseconds),
                                       totalMicroseconds,
                                       "Number of microseconds is negative.");

            _timeSpan = new TimeSpan(totalMicroseconds * TicksInMicrosecond);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTimeSpan"/> with the specified
        /// <see cref="TimeSpan"/> object.
        /// </summary>
        /// <param name="timeSpan">Time interval to initialize the <see cref="MetricTimeSpan"/>.</param>
        public MetricTimeSpan(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTimeSpan"/> with the specified
        /// numbers of hours, minutes and seconds.
        /// </summary>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="hours"/> is negative. -or-
        /// <paramref name="minutes"/> is negative. -or- <paramref name="seconds"/> is negative.</exception>
        public MetricTimeSpan(int hours, int minutes, int seconds)
            : this(hours, minutes, seconds, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTimeSpan"/> with the specified
        /// numbers of hours, minutes, seconds and milliseconds.
        /// </summary>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="hours"/> is negative. -or-
        /// <paramref name="minutes"/> is negative. -or- <paramref name="seconds"/> is negative. -or-
        /// <paramref name="milliseconds"/> is negative.</exception>
        public MetricTimeSpan(int hours, int minutes, int seconds, int milliseconds)
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
        /// Gets the value of the current <see cref="MetricTimeSpan"/> expressed in microseconds.
        /// </summary>
        public long TotalMicroseconds => _timeSpan.Ticks / TicksInMicrosecond;

        /// <summary>
        /// Gets the hours component of the time represented by the current <see cref="MetricTimeSpan"/>.
        /// </summary>
        public int Hours => _timeSpan.Hours;

        /// <summary>
        /// Gets the minutes component of the time represented by the current <see cref="MetricTimeSpan"/>.
        /// </summary>
        public int Minutes => _timeSpan.Minutes;

        /// <summary>
        /// Gets the seconds component of the time represented by the current <see cref="MetricTimeSpan"/>.
        /// </summary>
        public int Seconds => _timeSpan.Seconds;

        /// <summary>
        /// Gets the milliseconds component of the time represented by the current <see cref="MetricTimeSpan"/>.
        /// </summary>
        public int Milliseconds => _timeSpan.Milliseconds;

        #endregion

        #region Methods

        /// <summary>
        /// Converts the string representation of a metric time span to its <see cref="MetricTimeSpan"/>
        /// equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <param name="timeSpan">When this method returns, contains the <see cref="MetricTimeSpan"/>
        /// equivalent of the time span contained in <paramref name="input"/>, if the conversion succeeded, or
        /// null if the conversion failed. The conversion fails if the <paramref name="input"/> is null or
        /// <see cref="String.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns>true if <paramref name="input"/> was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string input, out MetricTimeSpan timeSpan)
        {
            return MetricTimeSpanParser.TryParse(input, out timeSpan).Status == ParsingStatus.Parsed;
        }

        /// <summary>
        /// Converts the string representation of a metric time span to its <see cref="MetricTimeSpan"/>
        /// equivalent.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <returns>A <see cref="MetricTimeSpan"/> equivalent to the time span contained in
        /// <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is null or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static MetricTimeSpan Parse(string input)
        {
            MetricTimeSpan timeSpan;
            var parsingResult = MetricTimeSpanParser.TryParse(input, out timeSpan);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return timeSpan;

            throw parsingResult.Exception;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Converts <see cref="TimeSpan"/> to <see cref="MetricTimeSpan"/>.
        /// </summary>
        /// <param name="timeSpan"><see cref="TimeSpan"/> to convert to <see cref="MetricTimeSpan"/>.</param>
        public static implicit operator MetricTimeSpan(TimeSpan timeSpan)
        {
            return new MetricTimeSpan(timeSpan);
        }

        /// <summary>
        /// Converts <see cref="MetricTimeSpan"/> to <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="timeSpan"><see cref="MetricTimeSpan"/> to convert to <see cref="TimeSpan"/>.</param>
        public static implicit operator TimeSpan(MetricTimeSpan timeSpan)
        {
            return timeSpan._timeSpan;
        }

        /// <summary>
        /// Determines if two <see cref="MetricTimeSpan"/> objects are equal.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
        /// <returns>true if time spans are equal, false otherwise.</returns>
        public static bool operator ==(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, timeSpan2))
                return true;

            if (ReferenceEquals(null, timeSpan1) || ReferenceEquals(null, timeSpan2))
                return false;

            return timeSpan1.TotalMicroseconds == timeSpan2.TotalMicroseconds;
        }

        /// <summary>
        /// Determines if two <see cref="MetricTimeSpan"/> objects are not equal.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
        /// <returns>false if time spans are equal, true otherwise.</returns>
        public static bool operator !=(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        /// <summary>
        /// Adds two specified <see cref="MetricTimeSpan"/> instances.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to add.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to add.</param>
        /// <returns>An object whose value is the sum of the values of <paramref name="timeSpan1"/> and
        /// <paramref name="timeSpan2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        public static MetricTimeSpan operator +(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return new MetricTimeSpan(timeSpan1.TotalMicroseconds + timeSpan2.TotalMicroseconds);
        }

        /// <summary>
        /// Subtracts a specified <see cref="MetricTimeSpan"/> from another one.
        /// </summary>
        /// <param name="timeSpan1">The minuend.</param>
        /// <param name="timeSpan2">The subtrahend.</param>
        /// <returns>An object whose value is the result of the value of <paramref name="timeSpan1"/> minus
        /// the value of <paramref name="timeSpan2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="timeSpan1"/> is less than <paramref name="timeSpan2"/>.</exception>
        public static MetricTimeSpan operator -(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            if (timeSpan1 < timeSpan2)
                throw new ArgumentException("First time span is less than second one.", nameof(timeSpan1));

            return new MetricTimeSpan(timeSpan1.TotalMicroseconds - timeSpan2.TotalMicroseconds);
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTimeSpan"/> is less than another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
        /// <returns>true if the value of <paramref name="timeSpan1"/> is less than the value of
        /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        public static bool operator <(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.TotalMicroseconds < timeSpan2.TotalMicroseconds;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTimeSpan"/> is greater than another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
        /// <returns>true if the value of <paramref name="timeSpan1"/> is greater than the value of
        /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        public static bool operator >(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.TotalMicroseconds > timeSpan2.TotalMicroseconds;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTimeSpan"/> is less than or equal to
        /// another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
        /// <returns>true if the value of <paramref name="timeSpan1"/> is less than or equal to the value of
        /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        public static bool operator <=(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.TotalMicroseconds <= timeSpan2.TotalMicroseconds;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTimeSpan"/> is greater than or equal to
        /// another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
        /// <returns>true if the value of <paramref name="timeSpan1"/> is greater than or equal to the value of
        /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        public static bool operator >=(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.TotalMicroseconds >= timeSpan2.TotalMicroseconds;
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
            return this == (obj as MetricTimeSpan);
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

        #region ITimeSpan

        /// <summary>
        /// Adds a time span to the current one.
        /// </summary>
        /// <remarks>
        /// If <paramref name="timeSpan"/> and the current time span have the same type,
        /// the result time span will be of this type too; otherwise - of the <see cref="MathTimeSpan"/>.
        /// </remarks>
        /// <param name="timeSpan">Time span to add to the current one.</param>
        /// <param name="mode">Mode of the operation that defines meaning of time spans the
        /// operation will be performed on.</param>
        /// <returns>Time span that is a sum of the <paramref name="timeSpan"/> and the
        /// current time span.</returns>
        /// <exception cref="ArgumentException"><paramref name="mode"/> is invalid.</exception>
        public ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var metricTimeSpan = timeSpan as MetricTimeSpan;
            return metricTimeSpan != null
                ? this + metricTimeSpan
                : TimeSpanUtilities.Add(this, timeSpan, mode);
        }

        /// <summary>
        /// Subtracts a time span from the current one.
        /// </summary>
        /// <remarks>
        /// If <paramref name="timeSpan"/> and the current time span have the same type,
        /// the result time span will be of this type too; otherwise - of the <see cref="MathTimeSpan"/>.
        /// </remarks>
        /// <param name="timeSpan">Time span to subtract from the current one.</param>
        /// <param name="mode">Mode of the operation that defines meaning of time spans the
        /// operation will be performed on.</param>
        /// <returns>Time span that is a difference between the <paramref name="timeSpan"/> and the
        /// current time span.</returns>
        public ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var metricTimeSpan = timeSpan as MetricTimeSpan;
            return metricTimeSpan != null
                ? this - metricTimeSpan
                : TimeSpanUtilities.Subtract(this, timeSpan, mode);
        }

        /// <summary>
        /// Stretches the current time span by multiplying its length by the specified multiplier.
        /// </summary>
        /// <param name="multiplier">Multiplier to stretch the time span by.</param>
        /// <returns>Time span that is the current time span stretched by the <paramref name="multiplier"/>.</returns>
        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MetricTimeSpan((long)Math.Round(TotalMicroseconds * multiplier));
        }

        /// <summary>
        /// Shrinks the current time span by dividing its length by the specified divisor.
        /// </summary>
        /// <param name="divisor">Divisor to shrink the time span by.</param>
        /// <returns>Time span that is the current time span shrinked by the <paramref name="divisor"/>.</returns>
        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

            return new MetricTimeSpan((long)Math.Round(TotalMicroseconds / divisor));
        }

        /// <summary>
        /// Clones the current time span.
        /// </summary>
        /// <returns>Copy of the current time span.</returns>
        public ITimeSpan Clone()
        {
            return new MetricTimeSpan(TotalMicroseconds);
        }

        #endregion
    }
}
