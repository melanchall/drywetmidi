using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Extension methods for notes managing.
    /// </summary>
    public static class NotesManagingUtilities
    {
        #region Methods

        /// <summary>
        /// Sets time and length of the specified note.
        /// </summary>
        /// <param name="note">Note to set time and length to.</param>
        /// <param name="time">Time to set to <paramref name="note"/>.</param>
        /// <param name="length">Length to set to <paramref name="note"/>.</param>
        /// <param name="tempoMap">Tempo map that will be used for time and length conversion.</param>
        /// <returns>An input <paramref name="note"/> with new time and length.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="note"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Note SetTimeAndLength(this Note note, ITimeSpan time, ITimeSpan length, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(note), note);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            note.Time = TimeConverter.ConvertFrom(time, tempoMap);
            note.Length = LengthConverter.ConvertFrom(length, note.Time, tempoMap);
            return note;
        }

        /// <summary>
        /// Creates an instance of the <see cref="NotesManager"/> initializing it with the
        /// specified events collection and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds notes to manage.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <returns>An instance of the <see cref="NotesManager"/> that can be used to manage
        /// notes represented by the <paramref name="eventsCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static NotesManager ManageNotes(this EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

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
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static NotesManager ManageNotes(this TrackChunk trackChunk, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.ManageNotes(sameTimeEventsComparison);
        }

        /// <summary>
        /// Gets notes contained in the specified collection of <see cref="MidiEvent"/>.
        /// </summary>
        /// <param name="events">Collection of<see cref="MidiFile"/> to search for notes.</param>
        /// <returns>Collection of notes contained in <paramref name="events"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is <c>null</c>.</exception>
        public static IEnumerable<Note> GetNotes(this IEnumerable<MidiEvent> events)
        {
            ThrowIfArgument.IsNull(nameof(events), events);

            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(events);

            return eventsCollection.ManageNotes().Notes.ToList();
        }

        /// <summary>
        /// Gets notes contained in the specified <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes.</param>
        /// <returns>Collection of notes contained in <paramref name="eventsCollection"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static IEnumerable<Note> GetNotes(this EventsCollection eventsCollection)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return eventsCollection.ManageNotes().Notes.ToList();
        }

        /// <summary>
        /// Gets notes contained in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes.</param>
        /// <returns>Collection of notes contained in <paramref name="trackChunk"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static IEnumerable<Note> GetNotes(this TrackChunk trackChunk)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.GetNotes();
        }

        /// <summary>
        /// Gets notes contained in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Track chunks to search for notes.</param>
        /// <returns>Collection of notes contained in <paramref name="trackChunks"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static IEnumerable<Note> GetNotes(this IEnumerable<TrackChunk> trackChunks)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return trackChunks.Where(c => c != null)
                              .SelectMany(GetNotes)
                              .OrderBy(n => n.Time)
                              .ToList();
        }

        /// <summary>
        /// Gets notes contained in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes.</param>
        /// <returns>Collection of notes contained in <paramref name="file"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        public static IEnumerable<Note> GetNotes(this MidiFile file)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().GetNotes();
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void ProcessNotes(this EventsCollection eventsCollection, Action<Note> action, Predicate<Note> match = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);

            using (var notesManager = eventsCollection.ManageNotes())
            {
                foreach (var note in notesManager.Notes.Where(n => match?.Invoke(n) != false))
                {
                    action(note);
                }
            }
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void ProcessNotes(this TrackChunk trackChunk, Action<Note> action, Predicate<Note> match = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            trackChunk.Events.ProcessNotes(action, match);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the collection of
        /// <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void ProcessNotes(this IEnumerable<TrackChunk> trackChunks, Action<Note> action, Predicate<Note> match = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk?.ProcessNotes(action, match);
            }
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void ProcessNotes(this MidiFile file, Action<Note> action, Predicate<Note> match = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);

            file.GetTrackChunks().ProcessNotes(action, match);
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static void RemoveNotes(this EventsCollection eventsCollection, Predicate<Note> match = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            using (var notesManager = eventsCollection.ManageNotes())
            {
                notesManager.Notes.RemoveAll(match ?? (n => true));
            }
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static void RemoveNotes(this TrackChunk trackChunk, Predicate<Note> match = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            trackChunk.Events.RemoveNotes(match);
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static void RemoveNotes(this IEnumerable<TrackChunk> trackChunks, Predicate<Note> match = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk?.RemoveNotes(match);
            }
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        public static void RemoveNotes(this MidiFile file, Predicate<Note> match = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            file.GetTrackChunks().RemoveNotes(match);
        }

        /// <summary>
        /// Adds collection of notes to the specified <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to add notes to.</param>
        /// <param name="notes">Notes to add to the <paramref name="eventsCollection"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="notes"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void AddNotes(this EventsCollection eventsCollection, IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(notes), notes);

            using (var notesManager = eventsCollection.ManageNotes())
            {
                notesManager.Notes.Add(notes);
            }
        }

        /// <summary>
        /// Adds collection of notes to the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to add notes to.</param>
        /// <param name="notes">Notes to add to the <paramref name="trackChunk"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="notes"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void AddNotes(this TrackChunk trackChunk, IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(notes), notes);

            trackChunk.Events.AddNotes(notes);
        }

        /// <summary>
        /// Creates a track chunk with the specified notes.
        /// </summary>
        /// <param name="notes">Collection of notes to create a track chunk.</param>
        /// <returns><see cref="TrackChunk"/> containing the specified notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
        public static TrackChunk ToTrackChunk(this IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            var trackChunk = new TrackChunk();
            trackChunk.AddNotes(notes);

            return trackChunk;
        }

        /// <summary>
        /// Creates a MIDI file with the specified notes.
        /// </summary>
        /// <param name="notes">Collection of notes to create a MIDI file.</param>
        /// <returns><see cref="MidiFile"/> containing the specified notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
        public static MidiFile ToFile(this IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            return new MidiFile(notes.ToTrackChunk());
        }

        /// <summary>
        /// Returns <see cref="MusicTheory.Note"/> corresponding to the specified <see cref="Note"/>.
        /// </summary>
        /// <param name="note"><see cref="Note"/> to get music theory note from.</param>
        /// <returns><see cref="MusicTheory.Note"/> corresponding to the <paramref name="note"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="note"/> is <c>null</c>.</exception>
        public static MusicTheory.Note GetMusicTheoryNote(this Note note)
        {
            ThrowIfArgument.IsNull(nameof(note), note);

            return note.UnderlyingNote;
        }

        #endregion
    }
}
