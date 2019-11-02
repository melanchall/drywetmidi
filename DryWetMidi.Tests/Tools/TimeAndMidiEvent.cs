using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    internal sealed class TimeAndMidiEvent
    {
        #region Constructor

        public TimeAndMidiEvent(ITimeSpan time, MidiEvent midiEvent)
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
