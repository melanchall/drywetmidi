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

        private IValueLineSource<TValue> _valueLineSource;
        private ValueLineSourceType _valueLineSourceType;

        #endregion

        #region Constructor

        public ValueLine(TValue defaultValue, ValueLineSourceType valueLineSourceType)
        {
            _valueLineSourceType = valueLineSourceType;

            switch (valueLineSourceType)
            {
                case ValueLineSourceType.Rbt:
                    _valueLineSource = new RbtValueLineSource<TValue>(defaultValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(valueLineSourceType), valueLineSourceType, null);
            }
        }

        #endregion

        #region Properties

        public int ValueChangesCount
        {
            get
            {
                return _valueLineSource.GetValueChangesCount();
            }
        }

        #endregion

        #region Methods

        public TValue GetValueAtTime(long time)
        {
            return _valueLineSource.GetValueAtTime(time);
        }

        public void SetValue(long time, TValue value)
        {
            _valueLineSource.SetValue(time, value);
            OnValuesChanged();
        }

        public void DeleteValues(long startTime)
        {
            DeleteValues(startTime, long.MaxValue);
        }

        public void DeleteValues(long startTime, long endTime)
        {
            _valueLineSource.DeleteValues(startTime, endTime);
            OnValuesChanged();
        }

        public void Clear()
        {
            _valueLineSource.Clear();
            OnValuesChanged();
        }

        public void ReplaceValues(ValueLine<TValue> valueLine)
        {
            _valueLineSource = _valueLineSource.Clone();
            OnValuesChanged();
        }

        public ValueLine<TValue> Reverse(long centerTime)
        {
            var maxTime = 2 * centerTime;
            var changes = this.TakeWhile(c => c.Time <= maxTime).ToArray();

            var values = new[] { _valueLineSource.DefaultValue }.Concat(changes.Select(c => c.Value)).Reverse();
            var times = new[] { 0L }.Concat(changes.Select(c => maxTime - c.Time).Reverse());

            var result = new ValueLine<TValue>(_valueLineSource.DefaultValue, _valueLineSourceType);

            foreach (var vc in values.Zip(times, (v, t) => new ValueChange<TValue>(t, v)))
            {
                result._valueLineSource.Add(vc);
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
            return _valueLineSource.GetEnumerator();
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
