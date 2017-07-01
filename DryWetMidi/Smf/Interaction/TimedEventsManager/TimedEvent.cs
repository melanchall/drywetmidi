using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents wrapper for the <see cref="MidiEvent"/> that provides absolute time of an event.
    /// </summary>
    public sealed class TimedEvent : ITimedObject
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
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

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
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Time is negative.");

                _time = value;
            }
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

        #endregion
    }
}
