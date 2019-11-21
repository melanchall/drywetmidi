using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

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
