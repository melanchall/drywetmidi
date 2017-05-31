using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class TimedEvent
    {
        #region Fields

        private long _time;

        #endregion

        #region Constructor

        public TimedEvent(MidiEvent midiEvent)
        {
            Event = midiEvent;
        }

        public TimedEvent(MidiEvent midiEvent, long time)
            : this(midiEvent)
        {
            Time = time;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

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
    }
}
