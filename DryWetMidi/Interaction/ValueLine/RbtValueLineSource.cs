using Melanchall.DryWetMidi.Common;
using System.Collections;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class RbtValueLineSource<TValue> : IValueLineSource<TValue>
    {
        private RedBlackTree<long, ValueChange<TValue>> _valueChanges = new RedBlackTree<long, ValueChange<TValue>>();

        public RbtValueLineSource(TValue defaultValue)
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
            var result = new RbtValueLineSource<TValue>(DefaultValue);
            result._valueChanges = _valueChanges.Clone();
            return result;
        }

        public void DeleteValues(long startTime, long endTime)
        {
            var node = _valueChanges.GetLastCoordinateBelowThreshold(startTime)
                ?? _valueChanges.GetMinimumCoordinate();

            while (true)
            {
                if (node == null || node.Value.Time > endTime)
                    break;

                var nextNode = _valueChanges.GetNextCoordinate(node);
                _valueChanges.Remove(node);
                node = nextNode;
            }
        }

        public TValue GetValueAtTime(long time)
        {
            var node = _valueChanges.GetFirstCoordinateAboveThreshold(time)
                ?? _valueChanges.GetMaximumCoordinate();

            if (node == null)
                return DefaultValue;

            node = node.Key > time
                ? _valueChanges.GetPreviousCoordinate(node)
                : node;

            return node != null
                ? node.Value.Value
                : DefaultValue;
        }

        public int GetValueChangesCount()
        {
            return _valueChanges.Count;
        }

        public void SetValue(long time, TValue value)
        {
            var node = _valueChanges.GetNodeByKey(time);
            if (node != null)
                _valueChanges.Remove(node);

            var coordinate = _valueChanges.GetLastCoordinateBelowThreshold(time);
            var currentValue = coordinate != null ? coordinate.Value.Value : DefaultValue;
            if (!value.Equals(currentValue))
                _valueChanges.Add(time, new ValueChange<TValue>(time, value));
        }

        public void Add(ValueChange<TValue> valueChange)
        {
            _valueChanges.Add(valueChange.Time, valueChange);
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
