using System;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class MidiEventReceivedEventArgs
    {
        #region Constructor

        internal MidiEventReceivedEventArgs(MidiEvent midiEvent, DateTime time)
        {
            Event = midiEvent;
            Time = time;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public DateTime Time { get; }

        #endregion
    }
}
