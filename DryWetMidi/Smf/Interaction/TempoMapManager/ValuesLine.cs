using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class ValuesLine<T> where T : class
    {
        #region Fields

        private readonly List<ValueChange<T>> _values = new List<ValueChange<T>>();
        private readonly T _defaultValue;

        #endregion

        #region Constructor

        public ValuesLine(T defaultValue)
        {
            _defaultValue = defaultValue;
        }

        #endregion

        #region Properties

        public IEnumerable<ValueChange<T>> Values => _values.OrderBy(v => v.Time);

        #endregion

        #region Methods

        public T AtTime(long time)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return _values.Where(p => p.Time <= time)
                          .OrderBy(p => p.Time)
                          .LastOrDefault()
                          ?.Value
                   ?? _defaultValue;
        }

        internal void SetValue(long time, T value)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            var currentValue = AtTime(time);
            if (currentValue.Equals(value))
                return;

            _values.RemoveAll(v => v.Time == time);
            _values.Add(new ValueChange<T>(time, value));
        }

        internal void DeleteValues(long startTime)
        {
            DeleteValues(startTime, long.MaxValue);
        }

        internal void DeleteValues(long startTime, long endTime)
        {
            if (startTime < 0)
                throw new ArgumentOutOfRangeException(nameof(startTime), startTime, "Start time is negative.");

            if (endTime < 0)
                throw new ArgumentOutOfRangeException(nameof(endTime), endTime, "End time is negative.");

            _values.RemoveAll(v => v.Time >= startTime && v.Time <= endTime);
        }

        #endregion
    }
}
