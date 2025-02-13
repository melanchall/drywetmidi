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

        private readonly RedBlackTree<long, ValueChange<TValue>> _valueChanges = new RedBlackTree<long, ValueChange<TValue>>();
        private readonly TValue _defaultValue;

        private bool _valuesChanged = true;

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
            var result = _defaultValue;

            var node = _valueChanges.GetLastNodeBelowThreshold(time)
                ?? _valueChanges.GetMinimumNode();

            while (node != null)
            {
                if (node.Value.Time > time)
                    break;

                result = node.Value.Value;
                node = _valueChanges.GetNextNode(node);
            }

            return result;
        }

        public void SetValue(long time, TValue value)
        {
            var node = _valueChanges.GetLastNodeBelowThreshold(time);
            var currentValue = node != null ? node.Value.Value : _defaultValue;

            node = node ?? _valueChanges.GetMinimumNode();

            while (node != null && node.Key <= time)
            {
                var nextNode = _valueChanges.GetNextNode(node);

                if (node.Key == time)
                    _valueChanges.Delete(node);

                node = nextNode;
            }

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
            var node = _valueChanges.GetLastNodeBelowThreshold(startTime)
                ?? _valueChanges.GetMinimumNode();

            while (true)
            {
                if (node == null || node.Value.Time > endTime)
                    break;

                var nextNode = _valueChanges.GetNextNode(node);
                _valueChanges.Delete(node);
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
            _valueChanges.Clear();

            foreach (var vc in valueLine._valueChanges)
            {
                _valueChanges.Add(vc.Time, vc);
            }

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
