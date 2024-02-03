using System;
using System.Collections.Generic;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents parameter (RPN or NRPN) encoded as series of Control Change events.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the channel of the current parameter. This channel is in fact
        /// the channel of Control Change events that represent the parameter.
        /// </summary>
        public FourBitNumber Channel { get; set; }

        /// <summary>
        /// Gets or sets the type of the current parameter's value.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public ParameterValueType ValueType
        {
            get { return _valueType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), _valueType);

                _valueType = value;
            }
        }

        /// <summary>
        /// Gets or sets absolute time of the parameter data in units defined by the time division of a MIDI file.
        /// </summary>
        /// <remarks>
        /// Note that the returned value will be in ticks (not seconds, not milliseconds and so on).
        /// Please read <see href="xref:a_time_length">Time and length</see> article to learn how you can
        /// get the time in different representations.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
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

        /// <summary>
        /// Clones object by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the object.</returns>
        public abstract ITimedObject Clone();

        /// <summary>
        /// Returns the collection of <see cref="TimedEvent"/> objects that represent the current
        /// parameter. In fact, each <see cref="TimedEvent"/> object will contain <see cref="ControlChangeEvent"/> event.
        /// </summary>
        /// <returns>Collection of <see cref="TimedEvent"/> objects that represent the current
        /// parameter.</returns>
        public abstract IEnumerable<TimedEvent> GetTimedEvents();

        #endregion
    }
}
