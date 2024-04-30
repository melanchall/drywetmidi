using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class ValueLine<TValue> : IEnumerable<ValueChange<TValue>>
    {
        #region Events

        internal event EventHandler ValuesChanged;

        #endregion

        #region Fields

        private readonly TimedObjectsComparer _comparer = new TimedObjectsComparer();
        private readonly List<ValueChange<TValue>> _valueChanges = new List<ValueChange<TValue>>();
        private readonly TValue _defaultValue;

        private bool _valuesChanged = true;
        private long _maxTime = long.MinValue;

        #endregion

        #region Constructor

        public ValueLine(TValue defaultValue)
        {
            _defaultValue = defaultValue;
        }

        #endregion

        #region Properties

        public int ValueChangesCount
        {
            get
            {
                return _valueChanges.Count;
            }
        }

        #endregion

        #region Methods

        public TValue GetValueAtTime(long time)
        {
            var valueChange = GetValueChangeAtTime(time);
            return valueChange != null ? valueChange.Value : _defaultValue;
        }

        public ValueChange<TValue> GetValueChangeAtTime(long time)
        {
            SortValueChanges();

            var result = default(ValueChange<TValue>);
            var valuesChangesCount = _valueChanges.Count;

            for (var i = 0; i < valuesChangesCount; i++)
            {
                var valueChange = _valueChanges[i];
                if (valueChange.Time > time)
                    break;

                result = valueChange;
            }

            return result;
        }

        public void SetValue(long time, TValue value)
        {
            var currentValue = GetValueAtTime(time);
            if (currentValue.Equals(value))
                return;

            var indexToRemove = -1;

            for (var i = _valueChanges.Count - 1; i >= 0; i--)
            {
                if (_valueChanges[i].Time != time)
                    continue;

                indexToRemove = i;
                break;
            }

            if (indexToRemove >= 0)
                _valueChanges.RemoveAt(indexToRemove);

            _valueChanges.Add(new ValueChange<TValue>(time, value));

            var forceSort = time < _maxTime;
            if (time > _maxTime)
                _maxTime = time;

            OnValuesChanged(forceSort);
        }

        public void DeleteValues(long startTime)
        {
            DeleteValues(startTime, long.MaxValue);
        }

        public void DeleteValues(long startTime, long endTime)
        {
            _valueChanges.RemoveAll(v => v.Time >= startTime && v.Time <= endTime);

            OnValuesChanged();
        }

        public void Clear()
        {
            _valueChanges.Clear();

            OnValuesChanged();
        }

        public void ReplaceValues(ValueLine<TValue> valueLine)
        {
            _valueChanges.Clear();
            _valueChanges.AddRange(valueLine._valueChanges);

            OnValuesChanged();
        }

        public ValueLine<TValue> Reverse(long centerTime)
        {
            var maxTime = 2 * centerTime;
            var changes = this.TakeWhile(c => c.Time <= maxTime).ToArray();

            var values = new[] { _defaultValue }.Concat(changes.Select(c => c.Value)).Reverse();
            var times = new[] { 0L }.Concat(changes.Select(c => maxTime - c.Time).Reverse());

            var result = new ValueLine<TValue>(_defaultValue);
            result._valueChanges.AddRange(values.Zip(times, (v, t) => new ValueChange<TValue>(t, v)));

            return result;
        }

        private void OnValuesChanged(bool forceSort = true)
        {
            if (forceSort)
                OnValueChangesNeedSorting();

            ValuesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnValueChangesNeedSorting()
        {
            _valuesChanged = true;
        }

        private void OnValueChangesSortingCompleted()
        {
            _valuesChanged = false;
        }

        private void SortValueChanges()
        {
            if (_valuesChanged)
            {
                _valueChanges.Sort(_comparer);
                OnValueChangesSortingCompleted();
            }
        }

        #endregion

        #region IEnumerable<ValueChange<TValue>>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ValueChange<TValue>> GetEnumerator()
        {
            SortValueChanges();
            return _valueChanges.GetEnumerator();
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
