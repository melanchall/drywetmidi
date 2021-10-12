using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Extension methods for chords managing.
    /// </summary>
    public static class ChordsManagingUtilities
    {
        #region Nested classes

        private interface IObjectDescriptor
        {
            bool ChordStart { get; }

            ITimedObject TimedObject { get; }
        }

        private interface IObjectDescriptorIndexed : IObjectDescriptor
        {
            Tuple<ITimedObject, int, int> IndexedTimedObject { get; }
        }

        private class TimedEventDescriptor : IObjectDescriptor
        {
            public TimedEventDescriptor(TimedEvent timedEvent)
            {
                TimedObject = timedEvent;
            }

            public bool ChordStart { get; } = false;

            public ITimedObject TimedObject { get; }
        }

        private class CompleteChordDescriptor : IObjectDescriptor
        {
            public CompleteChordDescriptor(Chord chord)
            {
                TimedObject = chord;
            }

            public bool ChordStart { get; } = false;

            public ITimedObject TimedObject { get; }
        }

        private sealed class TimedEventDescriptorIndexed : TimedEventDescriptor, IObjectDescriptorIndexed
        {
            private readonly int _index;

            public TimedEventDescriptorIndexed(TimedEvent timedEvent, int index)
                : base(timedEvent)
            {
                _index = index;
            }

            public Tuple<ITimedObject, int, int> IndexedTimedObject => Tuple.Create(TimedObject, _index, _index);
        }

        private class NoteDescriptor : IObjectDescriptor
        {
            public NoteDescriptor(Note note, bool chordStart)
            {
                TimedObject = note;
                ChordStart = chordStart;
            }

            public bool ChordStart { get; } = false;

            public ITimedObject TimedObject { get; }
        }

        private class NoteDescriptorIndexed : NoteDescriptor, IObjectDescriptorIndexed
        {
            private readonly int _noteOnIndex;
            private readonly int _noteOffIndex;

            public NoteDescriptorIndexed(Note note, int noteOnIndex, int noteOffIndex, bool chordStart)
                : base(note, chordStart)
            {
                _noteOnIndex = noteOnIndex;
                _noteOffIndex = noteOffIndex;
            }

            public Tuple<ITimedObject, int, int> IndexedTimedObject => Tuple.Create(TimedObject, _noteOnIndex, _noteOffIndex);
        }

        private sealed class ChordDescriptor
        {
            private readonly int _notesMinCount;

            public ChordDescriptor(long time, LinkedListNode<IObjectDescriptor> firstNoteNode, int notesMinCount)
            {
                Time = time;
                NotesNodes.Add(firstNoteNode);

                _notesMinCount = notesMinCount;
            }

            public long Time { get; }

            public List<LinkedListNode<IObjectDescriptor>> NotesNodes { get; }  = new List<LinkedListNode<IObjectDescriptor>>(3);

            public bool IsSealed { get; set; }

            public bool IsCompleted => NotesNodes.Count >= _notesMinCount;
        }

        private sealed class ChordDescriptorIndexed
        {
            private readonly int _notesMinCount;

            public ChordDescriptorIndexed(long time, LinkedListNode<IObjectDescriptorIndexed> firstNoteNode, int notesMinCount)
            {
                Time = time;
                NotesNodes.Add(firstNoteNode);

                _notesMinCount = notesMinCount;
            }

            public long Time { get; }

            public int EventsCollectionIndex { get; set; }

            public List<LinkedListNode<IObjectDescriptorIndexed>> NotesNodes { get; } = new List<LinkedListNode<IObjectDescriptorIndexed>>(3);

            public bool IsSealed { get; set; }

            public bool IsCompleted => NotesNodes.Count >= _notesMinCount;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets time and length of the specified chord.
        /// </summary>
        /// <param name="chord">Chord to set time and length to.</param>
        /// <param name="time">Time to set to <paramref name="chord"/>.</param>
        /// <param name="length">Length to set to <paramref name="chord"/>.</param>
        /// <param name="tempoMap">Tempo map that will be used for time and length conversion.</param>
        /// <returns>An input <paramref name="chord"/> with new time and length.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="chord"/> is <c>null</c>.</description>
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
        public static Chord SetTimeAndLength(this Chord chord, ITimeSpan time, ITimeSpan length, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            chord.Time = TimeConverter.ConvertFrom(time, tempoMap);
            chord.Length = LengthConverter.ConvertFrom(length, chord.Time, tempoMap);
            return chord;
        }

        /// <summary>
        /// Creates an instance of the <see cref="ChordsManager"/> initializing it with the
        /// specified events collection, notes tolerance and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds chords to manage.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <returns>An instance of the <see cref="ChordsManager"/> that can be used to manage chords
        /// represented by the <paramref name="eventsCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static ChordsManager ManageChords(this EventsCollection eventsCollection, ChordDetectionSettings settings = null, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return new ChordsManager(eventsCollection, settings, sameTimeEventsComparison);
        }

        /// <summary>
        /// Creates an instance of the <see cref="ChordsManager"/> initializing it with the
        /// events collection of the specified track chunk, notes tolerance and comparison delegate for events
        /// that have same time.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> that holds chords to manage.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <returns>An instance of the <see cref="ChordsManager"/> that can be used to manage
        /// chords represented by the <paramref name="trackChunk"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static ChordsManager ManageChords(this TrackChunk trackChunk, ChordDetectionSettings settings = null, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.ManageChords(settings, sameTimeEventsComparison);
        }

        /// <summary>
        /// Gets chords contained in the specified collection of <see cref="MidiEvent"/>.
        /// </summary>
        /// <param name="midiEvents">Collection of<see cref="MidiFile"/> to search for chords.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Collection of chords contained in <paramref name="midiEvents"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is <c>null</c>.</exception>
        public static ICollection<Chord> GetChords(this IEnumerable<MidiEvent> midiEvents, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            var result = new List<Chord>();

            foreach (var chord in GetChordsAndNotesAndTimedEventsLazy(midiEvents.GetTimedEventsLazy(), settings).OfType<Chord>())
            {
                result.Add(chord);
            }

            return result;
        }

        /// <summary>
        /// Gets chords contained in the specified <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for chords.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Collection of chords contained in <paramref name="eventsCollection"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static ICollection<Chord> GetChords(this EventsCollection eventsCollection, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            var result = new List<Chord>();
            var chordsBuilder = new ChordsBuilder(settings);

            var chords = chordsBuilder.GetChordsLazy(eventsCollection.GetTimedEventsLazy());

            result.AddRange(chords);
            return result;
        }

        /// <summary>
        /// Gets chords contained in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for chords.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Collection of chords contained in <paramref name="trackChunk"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static ICollection<Chord> GetChords(this TrackChunk trackChunk, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.GetChords(settings);
        }

        /// <summary>
        /// Gets chords contained in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Track chunks to search for chords.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Collection of chords contained in <paramref name="trackChunks"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static ICollection<Chord> GetChords(this IEnumerable<TrackChunk> trackChunks, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var eventsCollections = trackChunks.Select(c => c.Events).ToArray();

            switch (eventsCollections.Length)
            {
                case 0: return new Chord[0];
                case 1: return eventsCollections[0].GetChords(settings);
            }

            var eventsCount = eventsCollections.Sum(e => e.Count);
            var result = new List<Chord>(eventsCount / 3);
            var chordsBuilder = new ChordsBuilder(settings);

            var chords = chordsBuilder.GetChordsLazy(eventsCollections.GetTimedEventsLazy(eventsCount));

            result.AddRange(chords);
            return result;
        }

        /// <summary>
        /// Gets chords contained in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for chords.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Collection of chords contained in <paramref name="file"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        public static ICollection<Chord> GetChords(this MidiFile file, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().GetChords(settings);
        }

        /// <summary>
        /// Creates chords from notes.
        /// </summary>
        /// <param name="notes">Notes to create chords from.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Collection of chords made up from <paramref name="notes"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
        public static IEnumerable<Chord> GetChords(this IEnumerable<Note> notes, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            return notes.GetChordsAndNotesAndTimedEventsLazy(settings).OfType<Chord>().ToArray();
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of processed chords.</returns>
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
        public static int ProcessChords(this EventsCollection eventsCollection, Action<Chord> action, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);

            return eventsCollection.ProcessChords(action, chord => true, settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to process.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessChords(this EventsCollection eventsCollection, Action<Chord> action, Predicate<Chord> match, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return eventsCollection.ProcessChords(action, match, settings, true);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of processed chords.</returns>
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
        public static int ProcessChords(this TrackChunk trackChunk, Action<Chord> action, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunk.ProcessChords(action, chord => true, settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to process.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessChords(this TrackChunk trackChunk, Action<Chord> action, Predicate<Chord> match, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunk.Events.ProcessChords(action, match, settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the collection of
        /// <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of processed chords.</returns>
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
        public static int ProcessChords(this IEnumerable<TrackChunk> trackChunks, Action<Chord> action, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunks.ProcessChords(action, chord => true, settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the collection of
        /// <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to process.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessChords(this IEnumerable<TrackChunk> trackChunks, Action<Chord> action, Predicate<Chord> match, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunks.ProcessChords(action, match, settings, true);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of processed chords.</returns>
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
        public static int ProcessChords(this MidiFile file, Action<Chord> action, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);

            return file.ProcessChords(action, chord => true, settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to process.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessChords(this MidiFile file, Action<Chord> action, Predicate<Chord> match, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().ProcessChords(action, match, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Chord"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for chords to remove.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static int RemoveChords(this EventsCollection eventsCollection, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return eventsCollection.RemoveChords(chord => true, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Chord"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for chords to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to remove.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveChords(this EventsCollection eventsCollection, Predicate<Chord> match, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(match), match);

            var chordsToRemoveCount = eventsCollection.ProcessChords(
                c =>
                {
                    foreach (var note in c.Notes)
                    {
                        note.TimedNoteOnEvent.Event.Flag = note.TimedNoteOffEvent.Event.Flag = true;
                    }
                },
                match,
                settings,
                false);

            if (chordsToRemoveCount == 0)
                return 0;

            eventsCollection.RemoveTimedEvents(e => e.Event.Flag);
            return chordsToRemoveCount;
        }

        /// <summary>
        /// Removes all the <see cref="Chord"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for chords to remove.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static int RemoveChords(this TrackChunk trackChunk, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.RemoveChords(note => true, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Chord"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for chords to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to remove.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveChords(this TrackChunk trackChunk, Predicate<Chord> match, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunk.Events.RemoveChords(match, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Chord"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for chords to remove.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static int RemoveChords(this IEnumerable<TrackChunk> trackChunks, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return trackChunks.RemoveChords(note => true, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Chord"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for chords to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to remove.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveChords(this IEnumerable<TrackChunk> trackChunks, Predicate<Chord> match, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(match), match);

            var chordsToRemoveCount = trackChunks.ProcessChords(
                c =>
                {
                    foreach (var note in c.Notes)
                    {
                        note.TimedNoteOnEvent.Event.Flag = note.TimedNoteOffEvent.Event.Flag = true;
                    }
                },
                match,
                settings,
                false);

            if (chordsToRemoveCount == 0)
                return 0;

            trackChunks.RemoveTimedEvents(e => e.Event.Flag);
            return chordsToRemoveCount;
        }

        /// <summary>
        /// Removes all the <see cref="Chord"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for chords to remove.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        public static int RemoveChords(this MidiFile file, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.RemoveChords(chord => true, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Chord"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for chords to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to remove.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveChords(this MidiFile file, Predicate<Chord> match, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().RemoveChords(match, settings);
        }

        /// <summary>
        /// Returns <see cref="MusicTheory.Chord"/> containing notes of the specified <see cref="Chord"/>.
        /// </summary>
        /// <param name="chord"><see cref="Chord"/> to get music theory chord from.</param>
        /// <returns><see cref="MusicTheory.Chord"/> containing notes of the <paramref name="chord"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="chord"/> is <c>null</c>.</exception>
        public static MusicTheory.Chord GetMusicTheoryChord(this Chord chord)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            return new MusicTheory.Chord(chord.Notes.OrderBy(n => n.NoteNumber).Select(n => n.NoteName).ToArray());
        }

        internal static IEnumerable<Tuple<ITimedObject, int[]>> GetChordsAndNotesAndTimedEventsLazy(
            this IEnumerable<Tuple<TimedEvent, int>> timedEvents,
            ChordDetectionSettings settings)
        {
            settings = settings ?? new ChordDetectionSettings();

            var timedObjects = new LinkedList<IObjectDescriptorIndexed>();
            var chordsDescriptors = new LinkedList<ChordDescriptorIndexed>();
            var chordsDescriptorsByChannel = new LinkedListNode<ChordDescriptorIndexed>[FourBitNumber.MaxValue + 1];

            var notesTolerance = settings.NotesTolerance;
            var eventsCollectionShouldMatch = settings.ChordSearchContext == ChordSearchContext.SingleEventsCollection;

            foreach (var timedObjectTuple in timedEvents.GetNotesAndTimedEventsLazy(settings.NoteDetectionSettings ?? new NoteDetectionSettings()))
            {
                var timedObject = timedObjectTuple.Item1;

                var timedEvent = timedObject as TimedEvent;
                if (timedEvent != null)
                {
                    if (timedObjects.Count == 0)
                        yield return Tuple.Create(timedObject, new[] { timedObjectTuple.Item2 });
                    else
                        timedObjects.AddLast(new TimedEventDescriptorIndexed(timedEvent, timedObjectTuple.Item2));

                    continue;
                }

                var note = (Note)timedObject;
                var chordDescriptorNode = chordsDescriptorsByChannel[note.Channel];

                if (timedObjects.Count == 0 || chordDescriptorNode == null || chordDescriptorNode.List == null)
                {
                    CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, timedObjects, note, timedObjectTuple.Item2, timedObjectTuple.Item3, settings);
                }
                else
                {
                    var chordDescriptor = chordDescriptorNode.Value;
                    if (CanNoteBeAddedToChord(chordDescriptor, note, notesTolerance, timedObjectTuple.Item2, eventsCollectionShouldMatch))
                    {
                        var noteNode = timedObjects.AddLast(new NoteDescriptorIndexed(note, timedObjectTuple.Item2, timedObjectTuple.Item3, false));
                        chordDescriptor.NotesNodes.Add(noteNode);
                    }
                    else
                    {
                        chordDescriptor.IsSealed = true;

                        if (chordDescriptorNode.Previous == null)
                        {
                            foreach (var timedObjectX in GetTimedObjects(chordDescriptorNode, chordsDescriptors, timedObjects, true))
                            {
                                yield return timedObjectX;
                            }
                        }

                        CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, timedObjects, note, timedObjectTuple.Item2, timedObjectTuple.Item3, settings);
                    }
                }
            }

            foreach (var timedObject in GetTimedObjects(chordsDescriptors.First, chordsDescriptors, timedObjects, false))
            {
                yield return timedObject;
            }
        }

        internal static IEnumerable<ITimedObject> GetChordsAndNotesAndTimedEventsLazy(this IEnumerable<TimedEvent> timedEvents, ChordDetectionSettings settings)
        {
            settings = settings ?? new ChordDetectionSettings();

            return timedEvents
                .GetNotesAndTimedEventsLazy(settings.NoteDetectionSettings ?? new NoteDetectionSettings())
                .GetChordsAndNotesAndTimedEventsLazy(settings);
        }

        internal static IEnumerable<ITimedObject> GetChordsAndNotesAndTimedEventsLazy(this IEnumerable<ITimedObject> notesAndTimedEvents, ChordDetectionSettings settings)
        {
            return notesAndTimedEvents.GetChordsAndNotesAndTimedEventsLazy(settings, false);
        }

        internal static IEnumerable<ITimedObject> GetChordsAndNotesAndTimedEventsLazy(this IEnumerable<ITimedObject> notesAndTimedEvents, ChordDetectionSettings settings, bool chordsAllowed)
        {
            settings = settings ?? new ChordDetectionSettings();

            var timedObjects = new LinkedList<IObjectDescriptor>();
            var chordsDescriptors = new LinkedList<ChordDescriptor>();
            var chordsDescriptorsByChannel = new LinkedListNode<ChordDescriptor>[FourBitNumber.MaxValue + 1];

            foreach (var timedObject in notesAndTimedEvents)
            {
                if (chordsAllowed)
                {
                    var chord = timedObject as Chord;
                    if (chord != null)
                    {
                        if (timedObjects.Count == 0)
                            yield return chord;
                        else
                            timedObjects.AddLast(new CompleteChordDescriptor(chord));

                        continue;
                    }
                }

                var timedEvent = timedObject as TimedEvent;
                if (timedEvent != null)
                {
                    if (timedObjects.Count == 0)
                        yield return timedObject;
                    else
                        timedObjects.AddLast(new TimedEventDescriptor(timedEvent));

                    continue;
                }

                var note = (Note)timedObject;
                var chordDescriptorNode = chordsDescriptorsByChannel[note.Channel];

                if (timedObjects.Count == 0 || chordDescriptorNode == null || chordDescriptorNode.List == null)
                {
                    CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, timedObjects, note, settings);
                }
                else
                {
                    var chordDescriptor = chordDescriptorNode.Value;
                    if (CanNoteBeAddedToChord(chordDescriptor, note, settings.NotesTolerance))
                    {
                        var noteNode = timedObjects.AddLast(new NoteDescriptor(note, false));
                        chordDescriptor.NotesNodes.Add(noteNode);
                    }
                    else
                    {
                        chordDescriptor.IsSealed = true;

                        if (chordDescriptorNode.Previous == null)
                        {
                            foreach (var timedObjectX in GetTimedObjects(chordDescriptorNode, chordsDescriptors, timedObjects, true))
                            {
                                yield return timedObjectX;
                            }
                        }

                        CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, timedObjects, note, settings);
                    }
                }
            }

            foreach (var timedObject in GetTimedObjects(chordsDescriptors.First, chordsDescriptors, timedObjects, false))
            {
                yield return timedObject;
            }
        }

        internal static int ProcessChords(this EventsCollection eventsCollection, Action<Chord> action, Predicate<Chord> match, ChordDetectionSettings settings, bool canTimeOrLengthBeChanged)
        {
            settings = settings ?? new ChordDetectionSettings();

            var iMatched = 0;

            var timeOrLengthChanged = false;
            var collectedTimedEvents = canTimeOrLengthBeChanged ? new List<TimedEvent>(eventsCollection.Count) : null;

            var chordsBuilder = new ChordsBuilder(settings);
            var chords = chordsBuilder.GetChordsLazy(eventsCollection.GetTimedEventsLazy(false), canTimeOrLengthBeChanged, collectedTimedEvents);

            foreach (var chord in chords)
            {
                if (match(chord))
                {
                    var time = chord.Time;
                    var length = chord.Length;

                    action(chord);

                    timeOrLengthChanged |= chord.Time != time || chord.Length != length;

                    iMatched++;
                }
            }

            if (timeOrLengthChanged)
                eventsCollection.SortAndUpdateEvents(collectedTimedEvents);

            return iMatched;
        }

        internal static int ProcessChords(this IEnumerable<TrackChunk> trackChunks, Action<Chord> action, Predicate<Chord> match, ChordDetectionSettings settings, bool canTimeOrLengthBeChanged)
        {
            settings = settings ?? new ChordDetectionSettings();

            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            var iMatched = 0;

            var timeOrLengthChanged = false;
            var collectedTimedEvents = canTimeOrLengthBeChanged ? new List<Tuple<TimedEvent, int>>(eventsCount) : null;

            var chordsBuilder = new ChordsBuilder(settings);
            var chords = chordsBuilder.GetChordsLazy(eventsCollections.GetTimedEventsLazy(eventsCount, false), canTimeOrLengthBeChanged, collectedTimedEvents);

            foreach (var chord in chords)
            {
                if (match(chord))
                {
                    var time = chord.Time;
                    var length = chord.Length;

                    action(chord);

                    timeOrLengthChanged |= chord.Time != time || chord.Length != length;

                    iMatched++;
                }
            }

            if (timeOrLengthChanged)
                eventsCollections.SortAndUpdateEvents(collectedTimedEvents);

            return iMatched;
        }

        private static IEnumerable<ITimedObject> GetTimedObjects(
            LinkedListNode<ChordDescriptor> startChordDescriptorNode,
            LinkedList<ChordDescriptor> chordsDescriptors,
            LinkedList<IObjectDescriptor> timedObjects,
            bool getSealedOnly)
        {
            for (var chordDescriptorNode = startChordDescriptorNode; chordDescriptorNode != null;)
            {
                var chordDescriptor = chordDescriptorNode.Value;
                if (getSealedOnly && !chordDescriptor.IsSealed)
                    break;

                foreach (var noteNode in chordDescriptor.NotesNodes)
                {
                    timedObjects.Remove(noteNode);
                }

                if (chordDescriptor.IsCompleted)
                {
                    yield return new Chord(chordDescriptor.NotesNodes.Select(n => (Note)((NoteDescriptor)n.Value).TimedObject));
                }

                for (var node = timedObjects.First; node != null && !node.Value.ChordStart;)
                {
                    yield return node.Value.TimedObject;

                    var nextNode = node.Next;
                    timedObjects.Remove(node);
                    node = nextNode;
                }

                var nextChordDescriptorNode = chordDescriptorNode.Next;
                chordsDescriptors.Remove(chordDescriptorNode);
                chordDescriptorNode = nextChordDescriptorNode;
            }
        }

        private static IEnumerable<Tuple<ITimedObject, int[]>> GetTimedObjects(
            LinkedListNode<ChordDescriptorIndexed> startChordDescriptorNode,
            LinkedList<ChordDescriptorIndexed> chordsDescriptors,
            LinkedList<IObjectDescriptorIndexed> timedObjects,
            bool getSealedOnly)
        {
            for (var chordDescriptorNode = startChordDescriptorNode; chordDescriptorNode != null;)
            {
                var chordDescriptor = chordDescriptorNode.Value;
                if (getSealedOnly && !chordDescriptor.IsSealed)
                    break;

                if (chordDescriptor.IsCompleted)
                {
                    var notesCount = chordDescriptor.NotesNodes.Count;
                    var notes = new Note[notesCount];
                    var indices = new int[notesCount * 2];

                    for (var i = 0; i < notesCount; i++)
                    {
                        var noteNode = chordDescriptor.NotesNodes[i];
                        timedObjects.Remove(noteNode);

                        var objectDescriptor = chordDescriptor.NotesNodes[i].Value;

                        notes[i] = (Note)objectDescriptor.TimedObject;
                        indices[i * 2] = objectDescriptor.IndexedTimedObject.Item2;
                        indices[i * 2 + 1] = objectDescriptor.IndexedTimedObject.Item3;
                    }

                    yield return Tuple.Create((ITimedObject)new Chord(notes), indices);
                }

                for (var node = timedObjects.First; node != null && (!chordDescriptor.IsCompleted || !node.Value.ChordStart);)
                {
                    var timedObject = node.Value.IndexedTimedObject;
                    yield return Tuple.Create(timedObject.Item1, new[] { timedObject.Item2, timedObject.Item3 });

                    var nextNode = node.Next;
                    timedObjects.Remove(node);
                    node = nextNode;
                }

                var nextChordDescriptorNode = chordDescriptorNode.Next;
                chordsDescriptors.Remove(chordDescriptorNode);
                chordDescriptorNode = nextChordDescriptorNode;
            }
        }

        private static void CreateChordDescriptor(
            LinkedList<ChordDescriptor> chordsDescriptors,
            LinkedListNode<ChordDescriptor>[] chordsDescriptorsByChannel,
            LinkedList<IObjectDescriptor> timedObjects,
            Note note,
            ChordDetectionSettings settings)
        {
            var noteNode = timedObjects.AddLast(new NoteDescriptor(note, true));
            var chordDescriptor = new ChordDescriptor(note.Time, noteNode, settings.NotesMinCount);
            chordsDescriptorsByChannel[note.Channel] = chordsDescriptors.AddLast(chordDescriptor);
        }

        private static void CreateChordDescriptor(
            LinkedList<ChordDescriptorIndexed> chordsDescriptors,
            LinkedListNode<ChordDescriptorIndexed>[] chordsDescriptorsByChannel,
            LinkedList<IObjectDescriptorIndexed> timedObjects,
            Note note,
            int noteOnIndex,
            int noteOffIndex,
            ChordDetectionSettings settings)
        {
            var noteNode = timedObjects.AddLast(new NoteDescriptorIndexed(note, noteOnIndex, noteOffIndex, true));
            var chordDescriptor = new ChordDescriptorIndexed(note.Time, noteNode, settings.NotesMinCount) { EventsCollectionIndex = noteOnIndex };
            chordsDescriptorsByChannel[note.Channel] = chordsDescriptors.AddLast(chordDescriptor);
        }

        private static bool CanNoteBeAddedToChord(ChordDescriptor chordDescriptor, Note note, long notesTolerance)
        {
            return note.Time - chordDescriptor.Time <= notesTolerance;
        }

        private static bool CanNoteBeAddedToChord(ChordDescriptorIndexed chordDescriptor, Note note, long notesTolerance, int eventsCollectionIndex, bool eventsCollectionShouldMatch)
        {
            return note.Time - chordDescriptor.Time <= notesTolerance && (!eventsCollectionShouldMatch || chordDescriptor.EventsCollectionIndex == eventsCollectionIndex);
        }

        #endregion
    }
}
