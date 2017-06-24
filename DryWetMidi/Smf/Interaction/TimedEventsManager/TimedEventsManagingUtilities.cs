using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TimedEventsManagingUtilities
    {
        #region Methods

        public static TimedEventsManager ManageTimedEvents(this EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            return new TimedEventsManager(eventsCollection, sameTimeEventsComparison);
        }

        public static TimedEventsManager ManageTimedEvents(this TrackChunk trackChunk, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (trackChunk == null)
                throw new ArgumentNullException(nameof(trackChunk));

            return trackChunk.Events.ManageTimedEvents(sameTimeEventsComparison);
        }

        public static void AddEvent(this TimedEventsCollection eventsCollection, MidiEvent midiEvent, long time)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            eventsCollection.Add(new TimedEvent(midiEvent, time));
        }

        public static void AddEvent(this TimedEventsCollection eventsCollection, MidiEvent midiEvent, ITime time, TempoMap tempoMap)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            eventsCollection.AddEvent(midiEvent, TimeConverter.ConvertFrom(time, tempoMap));
        }

        #endregion
    }
}
