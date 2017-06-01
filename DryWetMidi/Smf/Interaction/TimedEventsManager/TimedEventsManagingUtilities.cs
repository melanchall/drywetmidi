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
            return trackChunk.Events.ManageTimedEvents(sameTimeEventsComparison);
        }

        #endregion
    }
}
