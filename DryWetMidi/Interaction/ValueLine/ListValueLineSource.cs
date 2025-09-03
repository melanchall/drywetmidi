using Melanchall.DryWetMidi.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class ListValueLineSource<TValue> : IValueLineSource<TValue>
    {
        private List<ValueChange<TValue>> _valueChanges = new List<ValueChange<TValue>>();

        public ListValueLineSource(TValue defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public TValue DefaultValue { get; }

        public void Clear()
        {
            _valueChanges.Clear();
        }

        public IValueLineSource<TValue> Clone()
        {
            var result = new ListValueLineSource<TValue>(DefaultValue);
            result._valueChanges = _valueChanges.ToList();
            return result;
        }

        public void DeleteValues(long startTime, long endTime)
        {
            int index;
            MathUtilities.GetLastElementBelowThreshold(
                _valueChanges,
                startTime,
                c => c.Time,
                out index);

            index++;

            while (index < _valueChanges.Count)
            {
                if (_valueChanges[index].Time > endTime)
                    break;

                _valueChanges.RemoveAt(index);
            }
        }

        public TValue GetValueAtTime(long time)
        {
            int index;
            MathUtilities.GetFirstElementAboveThreshold(
                _valueChanges,
                time,
                c => c.Time,
                out index);

            if (index < 0)
                index = _valueChanges.Count - 1;

            if (index < 0)
                return DefaultValue;

            index = _valueChanges[index].Time > time
                ? index - 1
                : index;

            return index >= 0
                ? _valueChanges[index].Value
                : DefaultValue;
        }

        public int GetValueChangesCount()
        {
            return _valueChanges.Count;
        }

        public void SetValue(long time, TValue value)
        {
            int index;
            MathUtilities.GetFirstElementAboveThreshold(
                _valueChanges,
                time,
                c => c.Time,
                out index);

            var valueChange = new ValueChange<TValue>(time, value);

            if (index < 0)
                index = _valueChanges.Count - 1;

            if (index >= 0 && _valueChanges[index].Time == time)
                _valueChanges.RemoveAt(index);

            MathUtilities.GetLastElementBelowThreshold(
                _valueChanges,
                time,
                c => c.Time,
                out index);

            var currentValue = index >= 0 ? _valueChanges[index].Value : DefaultValue;
            if (!value.Equals(currentValue))
                _valueChanges.Insert(index + 1, valueChange);
        }

        public void Add(ValueChange<TValue> valueChange)
        {
            int index;
            MathUtilities.GetFirstElementAboveThreshold(
                _valueChanges,
                valueChange.Time,
                c => c.Time,
                out index);

            if (index < 0)
                _valueChanges.Add(valueChange);
            else
                _valueChanges.Insert(index, valueChange);
        }

        public IEnumerator<ValueChange<TValue>> GetEnumerator()
        {
            return _valueChanges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
