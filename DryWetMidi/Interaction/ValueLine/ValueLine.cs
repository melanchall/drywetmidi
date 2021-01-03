using Melanchall.DryWetMidi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    // Do internal after O3 lifetime
    /// <summary>
    /// Represents timeline of a parameter's value.
    /// </summary>
    /// <typeparam name="TValue">Type of values.</typeparam>
    public sealed class ValueLine<TValue> : IEnumerable<ValueChange<TValue>>
    {
        #region Events

        internal event EventHandler ValuesChanged;

        #endregion

        #region Fields

        private readonly List<ValueChange<TValue>> _valuesChanges = new List<ValueChange<TValue>>();
        private readonly TValue _defaultValue;
        private readonly ValueChange<TValue> _defaultValueChange;

        private bool _valuesChanged = true;

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
            _defaultValueChange = new ValueChange<TValue>(0, defaultValue);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets value at specified time.
        /// </summary>
        /// <param name="time">Time to get a value at.</param>
        /// <returns>Parameter's value at the <paramref name="time"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        [Obsolete("OBS3")]
        public TValue AtTime(long time)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

            return GetValueAtTime(time);
        }

        internal TValue GetValueAtTime(long time)
        {
            var lastValueChange = _defaultValueChange;
            var valuesChangesCount = _valuesChanges.Count;

            for (var i = 0; i < valuesChangesCount; i++)
            {
                var valueChange = _valuesChanges[i];
                if (valueChange.Time > time)
                    break;

                lastValueChange = valueChange;
            }

            return lastValueChange.Value;
        }

        internal void SetValue(long time, TValue value)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(value), value);

            var currentValue = GetValueAtTime(time);
            if (currentValue.Equals(value))
                return;

            _valuesChanges.RemoveAll(v => v.Time == time);
            _valuesChanges.Add(new ValueChange<TValue>(time, value));

            OnValuesChanged();
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="startTime"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="endTime"/> is negative.</description>
        /// </item>
        /// </list>
        /// </exception>
        internal void DeleteValues(long startTime, long endTime)
        {
            ThrowIfTimeArgument.StartIsNegative(nameof(startTime), startTime);
            ThrowIfTimeArgument.EndIsNegative(nameof(endTime), endTime);

            _valuesChanges.RemoveAll(v => v.Time >= startTime && v.Time <= endTime);

            OnValuesChanged();
        }

        internal void Clear()
        {
            _valuesChanges.Clear();

            OnValuesChanged();
        }

        internal void ReplaceValues(ValueLine<TValue> valueLine)
        {
            _valuesChanges.Clear();
            _valuesChanges.AddRange(valueLine._valuesChanges);

            OnValuesChanged();
        }

        internal ValueLine<TValue> Reverse(long centerTime)
        {
            var maxTime = 2 * centerTime;
            var changes = this.TakeWhile(c => c.Time <= maxTime);

            var values = new[] { _defaultValue }.Concat(changes.Select(c => c.Value)).Reverse();
            var times = new[] { 0L }.Concat(changes.Select(c => maxTime - c.Time).Reverse());

            var result = new ValueLine<TValue>(_defaultValue);
            result._valuesChanges.AddRange(values.Zip(times, (v, t) => new ValueChange<TValue>(t, v)));

            return result;
        }

        private void OnValuesChanged()
        {
            OnValuesNeedSorting();
            ValuesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnValuesNeedSorting()
        {
            _valuesChanged = true;
        }

        private void OnValuesSortingCompleted()
        {
            _valuesChanged = false;
        }

        #endregion

        #region IEnumerable<ValueChange<TValue>>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ValueChange<TValue>> GetEnumerator()
        {
            if (_valuesChanged)
            {
                _valuesChanges.Sort(new TimedObjectsComparer<ValueChange<TValue>>());
                OnValuesSortingCompleted();
            }

            return _valuesChanges.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through
        /// the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
