using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class NotesManagingUtilities
    {
        #region Methods

        public static NotesManager ManageNotes(this EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            return new NotesManager(eventsCollection, sameTimeEventsComparison);
        }

        public static NotesManager ManageNotes(this TrackChunk trackChunk, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            return trackChunk.Events.ManageNotes(sameTimeEventsComparison);
        }

        #endregion
    }
}
