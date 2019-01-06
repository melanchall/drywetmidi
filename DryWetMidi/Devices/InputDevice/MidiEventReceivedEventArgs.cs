using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class MidiEventReceivedEventArgs
    {
        #region Constructor

        internal MidiEventReceivedEventArgs(MidiEvent midiEvent)
        {
            Event = midiEvent;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        #endregion
    }
}
