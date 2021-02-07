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
        #region Nested classes

        private sealed class NoteDescriptor
        {
            public NoteDescriptor(NoteId noteId, TimedEvent noteOnTimedEvent)
            {
                NoteId = noteId;
                NoteOnTimedEvent = noteOnTimedEvent;
            }

            public NoteId NoteId { get; }

            public TimedEvent NoteOnTimedEvent { get; }

            public TimedEvent NoteOffTimedEvent { get; set; }

            public bool IsCompleted => NoteOffTimedEvent != null;

            public Note GetNote()
            {
                return new Note(NoteOnTimedEvent, NoteOffTimedEvent);
            }
        }

        #endregion

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

        public static IEnumerable<Note> GetNotes(this IEnumerable<MidiEvent> midiEvents)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            var result = new List<Note>();

            foreach (var note in GetNotesLazy(midiEvents.GetTimedEventsLazy()))
            {
                result.Add(note);
            }

            return result;
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

            var result = new List<Note>(eventsCollection.Count / 2);

            foreach (var note in GetNotesLazy(eventsCollection.GetTimedEventsLazy()))
            {
                result.Add(note);
            }

            return result;
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

            var eventsCollections = trackChunks.Select(c => c.Events).ToArray();
            var eventsCount = eventsCollections.Sum(e => e.Count);

            var result = new List<Note>(eventsCount);

            foreach (var note in GetNotesLazy(eventsCollections.GetTimedEventsLazy(eventsCount).Select(e => e.Item1)))
            {
                result.Add(note);
            }

            return result;
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

        [Obsolete("OBS9")]
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

        [Obsolete("OBS9")]
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

        [Obsolete("OBS7")]
        /// <summary>
        /// Creates a track chunk with the specified notes.
        /// </summary>
        /// <param name="notes">Collection of notes to create a track chunk.</param>
        /// <returns><see cref="TrackChunk"/> containing the specified notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
        public static TrackChunk ToTrackChunk(this IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            return ((IEnumerable<ITimedObject>)notes).ToTrackChunk();
        }

        [Obsolete("OBS8")]
        /// <summary>
        /// Creates a MIDI file with the specified notes.
        /// </summary>
        /// <param name="notes">Collection of notes to create a MIDI file.</param>
        /// <returns><see cref="MidiFile"/> containing the specified notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
        public static MidiFile ToFile(this IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            return ((IEnumerable<ITimedObject>)notes).ToFile();
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

        internal static IEnumerable<Note> GetNotesLazy(IEnumerable<TimedEvent> timedEvents)
        {
            var notesDescriptors = new LinkedList<NoteDescriptor>();

            foreach (var timedEvent in timedEvents)
            {
                switch (timedEvent.Event.EventType)
                {
                    case MidiEventType.NoteOn:
                        {
                            notesDescriptors.AddLast(new NoteDescriptor(((NoteOnEvent)timedEvent.Event).GetNoteId(), timedEvent));
                        }
                        break;
                    case MidiEventType.NoteOff:
                        {
                            var noteId = ((NoteOffEvent)timedEvent.Event).GetNoteId();
                            var node = FindNoteDescriptorFromEnd(notesDescriptors, noteId);
                            if (node == null)
                                break;

                            node.Value.NoteOffTimedEvent = timedEvent;

                            var previousNode = node.Previous;
                            if (previousNode != null)
                                break;

                            for (var n = node; n != null;)
                            {
                                if (!n.Value.IsCompleted)
                                    break;

                                yield return n.Value.GetNote();
                                var next = n.Next;
                                notesDescriptors.Remove(n);
                                n = next;
                            }
                        }
                        break;
                }
            }

            foreach (var noteDescriptor in notesDescriptors)
            {
                if (!noteDescriptor.IsCompleted)
                    continue;

                yield return noteDescriptor.GetNote();
            }
        }

        private static LinkedListNode<NoteDescriptor> FindNoteDescriptorFromEnd(LinkedList<NoteDescriptor> notesDescriptors, NoteId noteId)
        {
            for (var node = notesDescriptors.Last; node != null; node = node.Previous)
            {
                if (node.Value.NoteId.Equals(noteId))
                    return node;
            }

            return null;
        }

        #endregion
    }
}
