using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    internal sealed class TimedEventInfo
    {
        #region Constructor

        public TimedEventInfo(MidiEvent midiEvent, ITimeSpan time)
        {
            Event = midiEvent;
            Time = time;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public ITimeSpan Time { get; }

        #endregion
    }
}
