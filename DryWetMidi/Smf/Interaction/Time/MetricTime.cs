using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
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

        public MetricTime(long totalMicroseconds)
        {
            if (totalMicroseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(totalMicroseconds),
                                                      totalMicroseconds,
                                                      "Number of microseconds is negative.");

            _timeSpan = new TimeSpan(totalMicroseconds * TicksInMicrosecond);
        }

        public MetricTime(int hours, int minutes, int seconds)
        {
            if (hours < 0)
                throw new ArgumentOutOfRangeException(nameof(hours), hours, "Number of hours is negative.");

            if (minutes < 0)
                throw new ArgumentOutOfRangeException(nameof(minutes), minutes, "Number of minutes is negative.");

            if (seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Number of seconds is negative.");

            _timeSpan = new TimeSpan(hours, minutes, seconds);
        }

        #endregion

        #region Properties

        public long TotalMicroseconds => _timeSpan.Ticks / TicksInMicrosecond;

        public int Hours => _timeSpan.Hours;

        public int Minutes => _timeSpan.Minutes;

        public int Seconds => _timeSpan.Seconds;

        public int Milliseconds => _timeSpan.Milliseconds;

        #endregion

        #region Methods

        public TimeSpan ToTimeSpan()
        {
            return new TimeSpan(_timeSpan.Ticks);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return _timeSpan.ToString(format, formatProvider);
        }

        public string ToString(string format)
        {
            return _timeSpan.ToString(format);
        }

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

        public static MetricTime operator +(MetricTime time)
        {
            return time;
        }

        public static MetricTime operator +(MetricTime time1, MetricTime time2)
        {
            return new MetricTime(time1.TotalMicroseconds + time2.TotalMicroseconds);
        }

        public static MetricTime operator -(MetricTime time1, MetricTime time2)
        {
            if (time1 < time2)
                throw new ArgumentException("First time is less than second one.", nameof(time1));

            return new MetricTime(time1.TotalMicroseconds - time2.TotalMicroseconds);
        }

        public static bool operator <(MetricTime time1, MetricTime time2)
        {
            return time1.TotalMicroseconds < time2.TotalMicroseconds;
        }

        public static bool operator >(MetricTime time1, MetricTime time2)
        {
            return time1.TotalMicroseconds > time2.TotalMicroseconds;
        }

        public static bool operator <=(MetricTime time1, MetricTime time2)
        {
            return time1.TotalMicroseconds <= time2.TotalMicroseconds;
        }

        public static bool operator >=(MetricTime time1, MetricTime time2)
        {
            return time1.TotalMicroseconds >= time2.TotalMicroseconds;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as MetricTime);
        }

        public override int GetHashCode()
        {
            return TotalMicroseconds.GetHashCode();
        }

        public override string ToString()
        {
            return _timeSpan.ToString();
        }

        #endregion
    }
}
