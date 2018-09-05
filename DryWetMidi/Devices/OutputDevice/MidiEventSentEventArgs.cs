using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class MidiEventSentEventArgs
    {
        #region Constructor

        internal MidiEventSentEventArgs(MidiEvent midiEvent)
        {
            Event = midiEvent;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        #endregion
    }
}
