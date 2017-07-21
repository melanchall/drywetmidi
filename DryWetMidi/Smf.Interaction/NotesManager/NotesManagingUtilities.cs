using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Extension methods for notes managing.
    /// </summary>
    public static class NotesManagingUtilities
    {
        #region Methods

        /// <summary>
        /// Creates an instance of the <see cref="NotesManager"/> initializing it with the
        /// specified events collection and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds notes to manage.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <returns>An instance of the <see cref="NotesManager"/> that can be used to manage
        /// notes represented by the <paramref name="eventsCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is null.</exception>
        public static NotesManager ManageNotes(this EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIf.ArgumentIsNull(nameof(eventsCollection), eventsCollection);

            return new NotesManager(eventsCollection, sameTimeEventsComparison);
        }

        /// <summary>
        /// Creates an instance of the <see cref="NotesManager"/> initializing it with the
        /// events collection of the specified track chunk and comparison delegate for events
        /// that have same time.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> that holds notes to manage.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <returns>An instance of the <see cref="NotesManager"/> that can be used to manage
        /// notes represented by the <paramref name="trackChunk"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null.</exception>
        public static NotesManager ManageNotes(this TrackChunk trackChunk, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIf.ArgumentIsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.ManageNotes(sameTimeEventsComparison);
        }

        public static IEnumerable<Note> GetNotes(this EventsCollection eventsCollection)
        {
            ThrowIf.ArgumentIsNull(nameof(eventsCollection), eventsCollection);

            return eventsCollection.ManageNotes().Notes;
        }

        /// <summary>
        /// Gets notes contained in the specified track chunk.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes.</param>
        /// <returns>Collection of notes contained in <paramref name="trackChunk"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null.</exception>
        public static IEnumerable<Note> GetNotes(this TrackChunk trackChunk)
        {
            ThrowIf.ArgumentIsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.GetNotes();
        }

        /// <summary>
        /// Gets notes contained in the specified track chunks.
        /// </summary>
        /// <param name="trackChunks">Track chunks to search for notes.</param>
        /// <returns>Collection of notes contained in <paramref name="trackChunks"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is null.</exception>
        public static IEnumerable<Note> GetNotes(this IEnumerable<TrackChunk> trackChunks)
        {
            ThrowIf.ArgumentIsNull(nameof(trackChunks), trackChunks);

            return trackChunks.Where(c => c != null)
                              .SelectMany(GetNotes)
                              .OrderBy(n => n.Time);
        }

        /// <summary>
        /// Gets notes contained in the specified MIDI file.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes.</param>
        /// <returns>Collection of notes contained in <paramref name="file"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is null.</exception>
        public static IEnumerable<Note> GetNotes(this MidiFile file)
        {
            ThrowIf.ArgumentIsNull(nameof(file), file);

            return file.GetTrackChunks().GetNotes();
        }

        public static void ProcessNotes(this EventsCollection eventsCollection, Action<Note> action, Predicate<Note> match = null)
        {
            ThrowIf.ArgumentIsNull(nameof(eventsCollection), eventsCollection);
            ThrowIf.ArgumentIsNull(nameof(action), action);

            using (var notesManager = eventsCollection.ManageNotes())
            {
                foreach (var note in notesManager.Notes.Where(n => match?.Invoke(n) != false))
                {
                    action(note);
                }
            }
        }

        public static void ProcessNotes(this TrackChunk trackChunk, Action<Note> action, Predicate<Note> match = null)
        {
            ThrowIf.ArgumentIsNull(nameof(trackChunk), trackChunk);
            ThrowIf.ArgumentIsNull(nameof(action), action);

            trackChunk.Events.ProcessNotes(action, match);
        }

        public static void ProcessNotes(this IEnumerable<TrackChunk> trackChunks, Action<Note> action, Predicate<Note> match = null)
        {
            ThrowIf.ArgumentIsNull(nameof(trackChunks), trackChunks);
            ThrowIf.ArgumentIsNull(nameof(action), action);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk?.ProcessNotes(action, match);
            }
        }

        public static void ProcessNotes(this MidiFile file, Action<Note> action, Predicate<Note> match = null)
        {
            ThrowIf.ArgumentIsNull(nameof(file), file);
            ThrowIf.ArgumentIsNull(nameof(action), action);

            file.GetTrackChunks().ProcessNotes(action, match);
        }

        public static void RemoveNotes(this EventsCollection eventsCollection, Predicate<Note> match = null)
        {
            ThrowIf.ArgumentIsNull(nameof(eventsCollection), eventsCollection);

            using (var notesManager = eventsCollection.ManageNotes())
            {
                notesManager.Notes.RemoveAll(match ?? (n => true));
            }
        }

        public static void RemoveNotes(this TrackChunk trackChunk, Predicate<Note> match = null)
        {
            ThrowIf.ArgumentIsNull(nameof(trackChunk), trackChunk);

            trackChunk.Events.RemoveNotes(match);
        }

        public static void RemoveNotes(this IEnumerable<TrackChunk> trackChunks, Predicate<Note> match = null)
        {
            ThrowIf.ArgumentIsNull(nameof(trackChunks), trackChunks);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk?.RemoveNotes(match);
            }
        }

        public static void RemoveNotes(this MidiFile file, Predicate<Note> match = null)
        {
            ThrowIf.ArgumentIsNull(nameof(file), file);

            file.GetTrackChunks().RemoveNotes(match);
        }

        #endregion
    }
}
