using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MetricLength : ILength
    {
        #region Fields

        private readonly MetricTime _time;

        #endregion

        #region Constructor

        public MetricLength(long totalMicroseconds)
        {
            _time = new MetricTime(totalMicroseconds);
        }

        public MetricLength(MetricTime time)
        {
            if (time == null)
                throw new ArgumentNullException(nameof(time));

            _time = new MetricTime(time.TotalMicroseconds);
        }

        public MetricLength(int hours, int minutes, int seconds)
            : this(hours, minutes, seconds, 0)
        {
        }

        public MetricLength(int hours, int minutes, int seconds, int milliseconds)
        {
            _time = new MetricTime(hours, minutes, seconds, milliseconds);
        }

        #endregion

        #region Properties

        public long TotalMicroseconds => _time.TotalMicroseconds;

        public int Hours => _time.Hours;

        public int Minutes => _time.Minutes;

        public int Seconds => _time.Seconds;

        public int Milliseconds => _time.Milliseconds;

        #endregion

        #region Methods

        public TimeSpan ToTimeSpan()
        {
            return _time.ToTimeSpan();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return _time.ToString(format, formatProvider);
        }

        public string ToString(string format)
        {
            return _time.ToString(format);
        }

        public bool Equals(MetricLength length)
        {
            if (ReferenceEquals(null, length))
                return false;

            if (ReferenceEquals(this, length))
                return true;

            return TotalMicroseconds == length.TotalMicroseconds;
        }

        #endregion

        #region Operators

        public static MetricLength operator +(MetricLength length)
        {
            return length;
        }

        public static MetricLength operator +(MetricLength length1, MetricLength length2)
        {
            return new MetricLength(length1.TotalMicroseconds + length2.TotalMicroseconds);
        }

        public static MetricLength operator -(MetricLength length1, MetricLength length2)
        {
            if (length1 < length2)
                throw new ArgumentException("First length is less than second one.", nameof(length1));

            return new MetricLength(length1.TotalMicroseconds - length2.TotalMicroseconds);
        }

        public static bool operator <(MetricLength length1, MetricLength length2)
        {
            return length1.TotalMicroseconds < length2.TotalMicroseconds;
        }

        public static bool operator >(MetricLength length1, MetricLength length2)
        {
            return length1.TotalMicroseconds > length2.TotalMicroseconds;
        }

        public static bool operator <=(MetricLength length1, MetricLength length2)
        {
            return length1.TotalMicroseconds <= length2.TotalMicroseconds;
        }

        public static bool operator >=(MetricLength length1, MetricLength length2)
        {
            return length1.TotalMicroseconds >= length2.TotalMicroseconds;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as MetricLength);
        }

        public override int GetHashCode()
        {
            return TotalMicroseconds.GetHashCode();
        }

        public override string ToString()
        {
            return _time.ToString();
        }

        #endregion
    }
}
