using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class NotesManagingUtilities
    {
        #region Methods

        public static NotesManager ManageNotes(this EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            return new NotesManager(eventsCollection, sameTimeEventsComparison);
        }

        public static NotesManager ManageNotes(this TrackChunk trackChunk, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (trackChunk == null)
                throw new ArgumentNullException(nameof(trackChunk));

            return trackChunk.Events.ManageNotes(sameTimeEventsComparison);
        }

        public static IEnumerable<Note> GetNotes(this IEnumerable<TrackChunk> trackChunks)
        {
            if (trackChunks == null)
                throw new ArgumentNullException(nameof(trackChunks));

            return trackChunks.SelectMany(c => c.ManageNotes().Notes)
                              .OrderBy(n => n.Time);
        }

        public static IEnumerable<Note> GetNotes(this MidiFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            return file.Chunks.OfType<TrackChunk>().GetNotes();
        }

        public static IEnumerable<Note> GetNotesAtTime(this TrackChunk trackChunk, long time, bool exactMatch = true)
        {
            if (trackChunk == null)
                throw new ArgumentNullException(nameof(trackChunk));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return trackChunk.ManageNotes().Notes.GetAtTime(time, exactMatch);
        }

        public static IEnumerable<Note> GetNotesAtTime(this IEnumerable<TrackChunk> trackChunks, long time, bool exactMatch = true)
        {
            if (trackChunks == null)
                throw new ArgumentNullException(nameof(trackChunks));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return trackChunks.SelectMany(c => c.GetNotesAtTime(time, exactMatch));
        }

        public static IEnumerable<Note> GetNotesAtTime(this MidiFile file, long time, bool exactMatch = true)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return file.Chunks.OfType<TrackChunk>().GetNotesAtTime(time, exactMatch);
        }

        #endregion
    }
}
