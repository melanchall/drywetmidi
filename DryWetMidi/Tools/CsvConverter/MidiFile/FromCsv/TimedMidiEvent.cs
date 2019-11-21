using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class TimedMidiEvent
    {
        #region Constructor

        public TimedMidiEvent(ITimeSpan time, MidiEvent midiEvent)
        {
            Time = time;
            Event = midiEvent;
        }

        #endregion

        #region Properties

        public ITimeSpan Time { get; }

        public MidiEvent Event { get; }

        #endregion
    }
}
