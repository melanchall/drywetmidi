using Melanchall.DryWetMidi.Common;
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

        private List<ValueChange<TValue>> _valueChanges = new List<ValueChange<TValue>>();
        private TValue _defaultValue;

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

        public IList<ValueChange<TValue>> GetValueChanges(
            long startTime,
            long endTime)
        {
            int index;
            MathUtilities.GetLastElementBelowThreshold(
                _valueChanges,
                startTime,
                c => c.Time,
                out index);

            index++;

            var result = new List<ValueChange<TValue>>();

            for (; index < _valueChanges.Count && _valueChanges[index].Time <= endTime; index++)
            {
                result.Add(_valueChanges[index]);
            }

            return result;
        }

        public TValue GetValueAtTime(long time)
        {
            if (_valueChanges.Count == 0)
                return _defaultValue;

            var lastValueChange = _valueChanges[_valueChanges.Count - 1];
            var lastTime = lastValueChange.Time;
            if (time >= lastTime)
                return lastValueChange.Value;

            int index;
            MathUtilities.GetFirstElementAboveThreshold(
                _valueChanges,
                time,
                c => c.Time,
                out index);

            if (index < 0)
                index = _valueChanges.Count - 1;

            if (_valueChanges[index].Time > time)
                index--;

            return index >= 0
                ? _valueChanges[index].Value
                : _defaultValue;
        }

        public bool SetValue(long time, TValue value)
        {
            var result = SetValueInternal(time, value);
            if (result)
                OnValuesChanged();
            
            return result;
        }

        public bool DeleteValues(long startTime)
        {
            return DeleteValues(startTime, long.MaxValue);
        }

        public bool DeleteValues(long startTime, long endTime)
        {
            if (_valueChanges.Count == 0)
                return false;

            var lastTime = _valueChanges[_valueChanges.Count - 1].Time;
            if (startTime > lastTime)
                return false;

            int index;
            MathUtilities.GetLastElementBelowThreshold(
                _valueChanges,
                startTime,
                c => c.Time,
                out index);

            var startIndex = ++index;
            var valueChangesCount = _valueChanges.Count;

            for (; index < valueChangesCount && _valueChanges[index].Time < endTime; index++)
            {
            }

            var count = index - startIndex;
            if (index < _valueChanges.Count)
            {
                var previousValue = startIndex > 0 ? _valueChanges[startIndex - 1].Value : _defaultValue;
                if (_valueChanges[index].Value.Equals(previousValue))
                    count++;
            }

            if (count == 0)
                return false;

            _valueChanges.RemoveRange(startIndex, count);
            OnValuesChanged();

            return true;
        }

        public void Clear()
        {
            var count = _valueChanges.Count;
            _valueChanges.Clear();

            if (count > 0)
                OnValuesChanged();
        }

        public void ReplaceValues(ValueLine<TValue> valueLine)
        {
            _defaultValue = valueLine._defaultValue;
            _valueChanges = valueLine.ToList();
            OnValuesChanged();
        }

        public ValueLine<TValue> Reverse(long centerTime)
        {
            var maxTime = 2 * centerTime;
            var changes = this.TakeWhile(c => c.Time <= maxTime).ToArray();

            var values = new[] { _defaultValue }.Concat(changes.Select(c => c.Value)).Reverse();
            var times = new[] { 0L }.Concat(changes.Select(c => maxTime - c.Time).Reverse());

            var result = new ValueLine<TValue>(_defaultValue);

            foreach (var vc in values.Zip(times, (v, t) => new ValueChange<TValue>(t, v)))
            {
                result._valueChanges.Add(vc);
            }

            return result;
        }

        private void OnValuesChanged()
        {
            ValuesChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool SetValueInternal(long time, TValue value)
        {
            var valueChange = new ValueChange<TValue>(time, value);

            // Simple case 1: there are no changes yet

            if (_valueChanges.Count == 0)
            {
                if (!value.Equals(_defaultValue))
                    _valueChanges.Add(valueChange);
                else
                    return false;

                return true;
            }

            // Simple case 2: new change is after the last one

            var lastTime = _valueChanges[_valueChanges.Count - 1].Time;
            if (time > lastTime)
            {
                var lastValue = _valueChanges[_valueChanges.Count - 1].Value;
                if (!value.Equals(lastValue))
                    _valueChanges.Add(valueChange);
                else
                    return false;

                return true;
            }

            // Common case

            int index;
            MathUtilities.GetFirstElementAboveThreshold(
                _valueChanges,
                time,
                c => c.Time,
                out index);

            var nextIndex = index;
            var nextValueChange = nextIndex >= 0 ? _valueChanges[nextIndex] : null;
            var nextValue = nextValueChange != null ? nextValueChange.Value : _defaultValue;

            var previousIndex = index > 0 ? index - 1 : (index < 0 ? _valueChanges.Count - 1 : -1);
            var previousValueChange = previousIndex >= 0 ? _valueChanges[previousIndex] : null;
            var previousValue = previousValueChange != null ? previousValueChange.Value : _defaultValue;
            var previousTime = previousValueChange != null ? previousValueChange.Time : long.MinValue;

            if (value.Equals(previousValue))
                return false;

            if (time > previousTime)
            {
                _valueChanges.Insert(nextIndex >= 0 ? nextIndex : _valueChanges.Count, valueChange);

                if (nextValueChange != null && value.Equals(nextValue))
                    _valueChanges.RemoveAt(nextIndex + 1);

                return true;
            }

            var previousPreviousValue = previousIndex > 0 ? _valueChanges[previousIndex - 1].Value : _defaultValue;
            if (!value.Equals(previousPreviousValue))
            {
                _valueChanges[previousIndex] = valueChange;

                if (nextValueChange != null && value.Equals(nextValue))
                    _valueChanges.RemoveAt(nextIndex);
            }
            else
            {
                _valueChanges.RemoveAt(previousIndex);

                if (nextValueChange != null && previousPreviousValue.Equals(nextValue))
                    _valueChanges.RemoveAt(nextIndex - 1);
            }

            return true;
        }

        #endregion

        #region IEnumerable<ValueChange<TValue>>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ValueChange<TValue>> GetEnumerator()
        {
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
