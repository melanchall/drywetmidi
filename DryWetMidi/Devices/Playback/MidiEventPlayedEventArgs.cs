using System;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class MidiEventPlayedEventArgs : EventArgs
    {
        #region Constructor

        internal MidiEventPlayedEventArgs(MidiEvent midiEvent)
        {
            Event = midiEvent;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        #endregion
    }
}
