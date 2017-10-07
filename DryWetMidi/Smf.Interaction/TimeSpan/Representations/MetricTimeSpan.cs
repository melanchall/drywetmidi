using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
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

        public MetricTimeSpan()
            : this(0)
        {
        }

        public MetricTimeSpan(long totalMicroseconds)
        {
            ThrowIfArgument.IsNegative(nameof(totalMicroseconds),
                                       totalMicroseconds,
                                       "Number of microseconds is negative.");

            _timeSpan = new TimeSpan(totalMicroseconds * TicksInMicrosecond);
        }

        public MetricTimeSpan(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        public MetricTimeSpan(int hours, int minutes, int seconds)
            : this(hours, minutes, seconds, 0)
        {
        }

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

        public static bool TryParse(string input, out MetricTimeSpan timeSpan)
        {
            return MetricTimeSpanParser.TryParse(input, out timeSpan).Status == ParsingStatus.Parsed;
        }

        public static MetricTimeSpan Parse(string input)
        {
            var parsingResult = MetricTimeSpanParser.TryParse(input, out var timeSpan);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return timeSpan;

            throw parsingResult.Exception;
        }

        #endregion

        #region Operators

        public static implicit operator MetricTimeSpan(TimeSpan timeSpan)
        {
            return new MetricTimeSpan(timeSpan);
        }

        public static implicit operator TimeSpan(MetricTimeSpan timeSpan)
        {
            return timeSpan._timeSpan;
        }

        public static bool operator ==(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, timeSpan2))
                return true;

            if (ReferenceEquals(null, timeSpan1) || ReferenceEquals(null, timeSpan2))
                return false;

            return timeSpan1.TotalMicroseconds == timeSpan2.TotalMicroseconds;
        }

        public static bool operator !=(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        public static MetricTimeSpan operator +(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return new MetricTimeSpan(timeSpan1.TotalMicroseconds + timeSpan2.TotalMicroseconds);
        }

        public static MetricTimeSpan operator -(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            if (timeSpan1 < timeSpan2)
                throw new ArgumentException("First time span is less than second one.", nameof(timeSpan1));

            return new MetricTimeSpan(timeSpan1.TotalMicroseconds - timeSpan2.TotalMicroseconds);
        }

        public static bool operator <(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.TotalMicroseconds < timeSpan2.TotalMicroseconds;
        }

        public static bool operator >(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.TotalMicroseconds > timeSpan2.TotalMicroseconds;
        }

        public static bool operator <=(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.TotalMicroseconds <= timeSpan2.TotalMicroseconds;
        }

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

        public ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var metricTimeSpan = timeSpan as MetricTimeSpan;
            return metricTimeSpan != null
                ? this + metricTimeSpan
                : TimeSpanUtilities.Add(this, timeSpan, mode);
        }

        public ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var metricTimeSpan = timeSpan as MetricTimeSpan;
            return metricTimeSpan != null
                ? this - metricTimeSpan
                : TimeSpanUtilities.Subtract(this, timeSpan, mode);
        }

        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MetricTimeSpan((long)Math.Round(TotalMicroseconds * multiplier));
        }

        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

            return new MetricTimeSpan((long)Math.Round(TotalMicroseconds / divisor));
        }

        #endregion
    }
}
