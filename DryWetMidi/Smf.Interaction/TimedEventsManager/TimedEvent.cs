using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents wrapper for the <see cref="MidiEvent"/> that provides absolute time of an event.
    /// </summary>
    public sealed class TimedEvent : ITimedObject, IEquatable<TimedEvent>
    {
        #region Fields

        private long _time;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedEvent"/> with the
        /// specified MIDI event.
        /// </summary>
        /// <param name="midiEvent">An event to wrap into <see cref="TimedEvent"/>.</param>
        public TimedEvent(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            Event = midiEvent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedEvent"/> with the
        /// specified MIDI event and absolute time.
        /// </summary>
        /// <param name="midiEvent">An event to wrap into <see cref="TimedEvent"/>.</param>
        /// <param name="time">Absolute time of an event in units defined by the time division of a MIDI file.</param>
        public TimedEvent(MidiEvent midiEvent, long time)
            : this(midiEvent)
        {
            Time = time;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets wrapped MIDI event.
        /// </summary>
        public MidiEvent Event { get; }

        /// <summary>
        /// Gets or sets absolute time of the event in units defined by the time division of a MIDI file.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value is negative.</exception>
        public long Time
        {
            get { return _time; }
            set
            {
                ThrowIfTimeArgument.IsNegative(nameof(value), value);

                _time = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified timed event is equal to the current one.
        /// </summary>
        /// <param name="timedEvent">The timed event to compare with the current one.</param>
        /// <param name="respectTime">If true the time will be taken into an account while comparing
        /// events; if false - times will be ignored.</param>
        /// <returns>true if the specified timed event is equal to the current one; otherwise, false.</returns>
        public bool Equals(TimedEvent timedEvent, bool respectTime)
        {
            if (ReferenceEquals(null, timedEvent))
                return false;

            if (ReferenceEquals(this, timedEvent))
                return true;

            return Event.Equals(timedEvent.Event, false) && (!respectTime || Time == timedEvent.Time);
        }

        #endregion

        #region IEquatable<TimedEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="timedEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(TimedEvent timedEvent)
        {
            return Equals(timedEvent, true);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Event at {Time}: {Event}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TimedEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Event.GetHashCode() ^ Time.GetHashCode();
        }

        #endregion
    }
}
