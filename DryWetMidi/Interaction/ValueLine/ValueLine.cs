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

        private RedBlackTree<long, ValueChange<TValue>> _valueChanges = new RedBlackTree<long, ValueChange<TValue>>();
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

        public TValue GetValueAtTime(long time)
        {
            var node = _valueChanges.GetFirstCoordinateAboveThreshold(time)
                ?? _valueChanges.GetMaximumCoordinate();
            
            if (node == null)
                return _defaultValue;

            node = node.Key > time
                ? _valueChanges.GetPreviousCoordinate(node)
                : node;

            return node != null
                ? node.Value.Value
                : _defaultValue;
        }

        public void SetValue(long time, TValue value)
        {
            var node = _valueChanges.GetNodeByKey(time);
            if (node != null)
                _valueChanges.Remove(node);

            var coordinate = _valueChanges.GetLastCoordinateBelowThreshold(time);
            var currentValue = coordinate != null ? coordinate.Value.Value : _defaultValue;
            if (!value.Equals(currentValue))
                _valueChanges.Add(time, new ValueChange<TValue>(time, value));

            OnValuesChanged();
        }

        public void DeleteValues(long startTime)
        {
            DeleteValues(startTime, long.MaxValue);
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

            OnValuesChanged();
        }

        public void Clear()
        {
            _valueChanges.Clear();

            OnValuesChanged();
        }

        public void ReplaceValues(ValueLine<TValue> valueLine)
        {
            _valueChanges = valueLine._valueChanges.Clone();
            _defaultValue = valueLine._defaultValue;

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
                result._valueChanges.Add(vc.Time, vc);
            }

            return result;
        }

        private void OnValuesChanged()
        {
            ValuesChanged?.Invoke(this, EventArgs.Empty);
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
