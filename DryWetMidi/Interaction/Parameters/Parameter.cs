using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    // TODO: override Equals, GetHashCode, == and so on (see TimedEvent for ref)
    public abstract class Parameter : ITimedObject, INotifyTimeChanged
    {
        #region Events

        /// <summary>
        /// Occurs when the time of an object has been changed.
        /// </summary>
        public event EventHandler<TimeChangedEventArgs> TimeChanged;

        #endregion

        #region Fields

        private long _time;
        private ParameterValueType _valueType = ParameterValueType.Exact;

        #endregion

        #region Properties

        public FourBitNumber Channel { get; set; }

        public ParameterValueType ValueType
        {
            get { return _valueType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), _valueType);

                _valueType = value;
            }
        }

        public long Time
        {
            get { return _time; }
            set
            {
                ThrowIfTimeArgument.IsNegative(nameof(value), value);

                var oldTime = Time;
                if (value == oldTime)
                    return;

                _time = value;

                TimeChanged?.Invoke(this, new TimeChangedEventArgs(oldTime, value));
            }
        }

        #endregion

        #region Methods

        public abstract IEnumerable<TimedEvent> GetTimedEvents();

        #endregion
    }
}
