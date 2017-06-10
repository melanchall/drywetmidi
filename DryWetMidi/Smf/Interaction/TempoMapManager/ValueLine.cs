using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class ValueLine<TValue> where TValue : class
    {
        #region Fields

        private readonly List<ValueChange<TValue>> _values = new List<ValueChange<TValue>>();
        private readonly TValue _defaultValue;

        #endregion

        #region Constructor

        public ValueLine(TValue defaultValue)
        {
            _defaultValue = defaultValue;
        }

        #endregion

        #region Properties

        public IEnumerable<ValueChange<TValue>> Values => _values.OrderBy(v => v.Time);

        #endregion

        #region Methods

        public TValue AtTime(long time)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return _values.Where(p => p.Time <= time)
                          .OrderBy(p => p.Time)
                          .LastOrDefault()
                          ?.Value
                   ?? _defaultValue;
        }

        internal void SetValue(long time, TValue value)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            var currentValue = AtTime(time);
            if (currentValue.Equals(value))
                return;

            _values.RemoveAll(v => v.Time == time);
            _values.Add(new ValueChange<TValue>(time, value));
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
