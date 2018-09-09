using System;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class RecordingEvent
    {
        #region Constructor

        public RecordingEvent(MidiEvent midiEvent, TimeSpan time)
        {
            Event = midiEvent;
            Time = time;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public TimeSpan Time { get; }

        #endregion
    }
}
