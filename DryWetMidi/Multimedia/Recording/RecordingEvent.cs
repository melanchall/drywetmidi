using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class RecordingEvent
    {
        #region Constructor

        public RecordingEvent(MidiEvent midiEvent, long timeNs)
        {
            Event = midiEvent;
            TimeNs = timeNs;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public long TimeNs { get; }

        #endregion
    }
}
