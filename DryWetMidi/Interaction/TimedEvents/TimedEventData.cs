using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class TimedEventData
    {
        #region Constructor

        internal TimedEventData(MidiEvent midiEvent, long time, int eventsCollectionIndex, int eventIndex)
        {
            Event = midiEvent;
            Time = time;
            EventsCollectionIndex = eventsCollectionIndex;
            EventIndex = eventIndex;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public long Time { get; }

        public int EventsCollectionIndex { get; }

        public int EventIndex { get; }

        #endregion
    }
}
