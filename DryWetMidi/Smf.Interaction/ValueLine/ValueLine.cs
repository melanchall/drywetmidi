using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents timeline of a parameter's value.
    /// </summary>
    /// <typeparam name="TValue">Type of values.</typeparam>
    public sealed class ValueLine<TValue> where TValue : class
    {
        #region Fields

        private readonly List<ValueChange<TValue>> _values = new List<ValueChange<TValue>>();
        private readonly TValue _defaultValue;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueLine{TValue}"/> with the specified
        /// default value.
        /// </summary>
        /// <param name="defaultValue">Default value of a parameter.</param>
        internal ValueLine(TValue defaultValue)
        {
            _defaultValue = defaultValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets collection of all changes of a parameter's value.
        /// </summary>
        public IEnumerable<ValueChange<TValue>> Values => _values.OrderBy(v => v.Time);

        #endregion

        #region Methods

        /// <summary>
        /// Gets value at specified time.
        /// </summary>
        /// <param name="time">Time to get a value at.</param>
        /// <returns>Parameter's value at the <paramref name="time"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
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

        /// <summary>
        /// Sets new value at specified time that will last until next value change.
        /// </summary>
        /// <param name="time">Time parameter's value should be changed at.</param>
        /// <param name="value">New parameter's value that will last until next value change.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        internal void SetValue(long time, TValue value)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var currentValue = AtTime(time);
            if (currentValue.Equals(value))
                return;

            _values.RemoveAll(v => v.Time == time);
            _values.Add(new ValueChange<TValue>(time, value));
        }

        /// <summary>
        /// Deletes all parameter's value changes after the specified time.
        /// </summary>
        /// <param name="startTime">Time value changes should be deleted after.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is negative.</exception>
        internal void DeleteValues(long startTime)
        {
            DeleteValues(startTime, long.MaxValue);
        }

        /// <summary>
        /// Deletes all parameter's value changes in the specified time range.
        /// </summary>
        /// <param name="startTime">Start time of the time range where value changes should be deleted.</param>
        /// <param name="endTime">End time of the time range where value changes should be deleted.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is negative. -or-
        /// <paramref name="endTime"/> is negative.</exception>
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
