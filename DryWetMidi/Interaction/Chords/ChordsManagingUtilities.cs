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

        private interface IChordDescriptor
        {
            bool IsSealed { get; set; }

            bool IsCompleted { get; }
        }

        private interface IObjectDescriptor
        {
            bool ChordStart { get; }

            IChordDescriptor ChordDescriptor { get; set; }

            ITimedObject TimedObject { get; }
        }

        private interface IObjectDescriptorIndexed : IObjectDescriptor
        {
            TimedObjectAt<ITimedObject> IndexedTimedObject { get; }
        }

        private class TimedEventDescriptor : IObjectDescriptor
        {
            public TimedEventDescriptor(TimedEvent timedEvent)
            {
                TimedObject = timedEvent;
            }

            public bool ChordStart { get; } = false;

            public ITimedObject TimedObject { get; }

            public IChordDescriptor ChordDescriptor { get; set; }
        }

        private class CompleteChordDescriptor : IObjectDescriptor
        {
            public CompleteChordDescriptor(Chord chord)
            {
                TimedObject = chord;
            }

            public bool ChordStart { get; } = false;

            public IChordDescriptor ChordDescriptor { get; set; }

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

            public TimedObjectAt<ITimedObject> IndexedTimedObject => new TimedObjectAt<ITimedObject>(TimedObject, _index);
        }

        private class NoteDescriptor : IObjectDescriptor
        {
            public NoteDescriptor(Note note, bool chordStart)
            {
                TimedObject = note;
                ChordStart = chordStart;
            }

            public bool ChordStart { get; } = false;

            public IChordDescriptor ChordDescriptor { get; set; }

            public ITimedObject TimedObject { get; }
        }

        private class NoteDescriptorIndexed : NoteDescriptor, IObjectDescriptorIndexed
        {
            private readonly int _noteOnIndex;

            public NoteDescriptorIndexed(Note note, int noteOnIndex, bool chordStart)
                : base(note, chordStart)
            {
                _noteOnIndex = noteOnIndex;
            }

            public TimedObjectAt<ITimedObject> IndexedTimedObject => new TimedObjectAt<ITimedObject>(TimedObject, _noteOnIndex);
        }

        private sealed class ChordDescriptor : IChordDescriptor
        {
            private readonly int _notesMinCount;

            public ChordDescriptor(long time, LinkedListNode<IObjectDescriptor> firstNoteNode, int notesMinCount)
            {
                Time = time;
                NotesNodes.Add(firstNoteNode);

                _notesMinCount = notesMinCount;
            }

            public long Time { get; }

            public List<LinkedListNode<IObjectDescriptor>> NotesNodes { get; } = new List<LinkedListNode<IObjectDescriptor>>(3);

            public bool IsSealed { get; set; }

            public bool IsCompleted => NotesNodes.Count >= _notesMinCount;
        }

        private sealed class ChordDescriptorIndexed : IChordDescriptor
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
        /// Creates an instance of the <see cref="TimedObjectsManager{Chord}"/> initializing it with the
        /// specified events collection. More info in the <see href="xref:a_managers">Objects managers</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds chords to manage.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <param name="comparer">Comparer that will be used to order objects on enumerating and saving objects
        /// back to the <paramref name="eventsCollection"/> via <see cref="TimedObjectsManager{TObject}.SaveChanges"/>
        /// or <see cref="TimedObjectsManager{TObject}.Dispose()"/>.</param>
        /// <returns>An instance of the <see cref="TimedObjectsManager{Chord}"/> that can be used to manage chords
        /// represented by the <paramref name="eventsCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static TimedObjectsManager<Chord> ManageChords(
            this EventsCollection eventsCollection,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            TimedObjectsComparer comparer = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return new TimedObjectsManager<Chord>(
                eventsCollection,
                new ObjectDetectionSettings
                {
                    ChordDetectionSettings = settings,
                    NoteDetectionSettings = noteDetectionSettings,
                    TimedEventDetectionSettings = timedEventDetectionSettings,
                },
                comparer);
        }

        /// <summary>
        /// Creates an instance of the <see cref="TimedObjectsManager{Chord}"/> initializing it with the
        /// events collection of the specified track chunk.  More info in the
        /// <see href="xref:a_managers">Objects managers</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> that holds chords to manage.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <param name="comparer">Comparer that will be used to order objects on enumerating and saving objects
        /// back to the <paramref name="trackChunk"/> via <see cref="TimedObjectsManager{TObject}.SaveChanges"/>
        /// or <see cref="TimedObjectsManager{TObject}.Dispose()"/>.</param>
        /// <returns>An instance of the <see cref="TimedObjectsManager{Chord}"/> that can be used to manage
        /// chords represented by the <paramref name="trackChunk"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static TimedObjectsManager<Chord> ManageChords(
            this TrackChunk trackChunk,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            TimedObjectsComparer comparer = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.ManageChords(settings, noteDetectionSettings, timedEventDetectionSettings, comparer);
        }

        /// <summary>
        /// Gets chords contained in the specified collection of <see cref="MidiEvent"/>. More info in the
        /// <see href="xref:a_getting_objects#getchords">Getting objects: GetChords</see> article.
        /// </summary>
        /// <param name="midiEvents">Collection of<see cref="MidiFile"/> to search for chords.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Collection of chords contained in <paramref name="midiEvents"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is <c>null</c>.</exception>
        public static ICollection<Chord> GetChords(
            this IEnumerable<MidiEvent> midiEvents,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            var result = new List<Chord>();

            foreach (var chord in GetChordsAndNotesAndTimedEventsLazy(midiEvents.GetTimedEventsLazy(timedEventDetectionSettings), settings, noteDetectionSettings, timedEventDetectionSettings).OfType<Chord>())
            {
                result.Add(chord);
            }

            return new SortedTimedObjectsImmutableCollection<Chord>(result);
        }

        /// <summary>
        /// Gets chords contained in the specified <see cref="EventsCollection"/>. More info in the
        /// <see href="xref:a_getting_objects#getchords">Getting objects: GetChords</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for chords.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Collection of chords contained in <paramref name="eventsCollection"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessChords(EventsCollection, Action{Chord}, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)"/>
        /// <seealso cref="ProcessChords(EventsCollection, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)"/>
        /// <seealso cref="RemoveChords(EventsCollection, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="RemoveChords(EventsCollection, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<Chord> GetChords(
            this EventsCollection eventsCollection,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            var result = new List<Chord>();
            var chordsBuilder = new ChordsBuilder(settings, noteDetectionSettings);

            var chords = chordsBuilder.GetChordsLazy(new[] { eventsCollection }.GetTimedEventsLazy(eventsCollection.Count, timedEventDetectionSettings));

            result.AddRange(chords.Select(c => c.Object));
            return new SortedTimedObjectsImmutableCollection<Chord>(result);
        }

        /// <summary>
        /// Gets chords contained in the specified <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_getting_objects#getchords">Getting objects: GetChords</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for chords.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Collection of chords contained in <paramref name="trackChunk"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessChords(TrackChunk, Action{Chord}, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)"/>
        /// <seealso cref="ProcessChords(TrackChunk, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)"/>
        /// <seealso cref="RemoveChords(TrackChunk, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="RemoveChords(TrackChunk, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<Chord> GetChords(
            this TrackChunk trackChunk,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.GetChords(settings, noteDetectionSettings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Gets chords contained in the specified collection of <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_getting_objects#getchords">Getting objects: GetChords</see> article.
        /// </summary>
        /// <param name="trackChunks">Track chunks to search for chords.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Collection of chords contained in <paramref name="trackChunks"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessChords(IEnumerable{TrackChunk}, Action{Chord}, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)"/>
        /// <seealso cref="ProcessChords(IEnumerable{TrackChunk}, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)"/>
        /// <seealso cref="RemoveChords(IEnumerable{TrackChunk}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="RemoveChords(IEnumerable{TrackChunk}, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<Chord> GetChords(
            this IEnumerable<TrackChunk> trackChunks,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var eventsCollections = trackChunks.Select(c => c.Events).ToArray();

            switch (eventsCollections.Length)
            {
                case 0: return new Chord[0];
                case 1: return eventsCollections[0].GetChords(settings, noteDetectionSettings, timedEventDetectionSettings);
            }

            var eventsCount = eventsCollections.Sum(e => e.Count);
            var result = new List<Chord>(eventsCount / 3);
            var chordsBuilder = new ChordsBuilder(settings, noteDetectionSettings);

            var chords = chordsBuilder.GetChordsLazy(eventsCollections.GetTimedEventsLazy(eventsCount, timedEventDetectionSettings));

            result.AddRange(chords.Select(c => c.Object));
            return new SortedTimedObjectsImmutableCollection<Chord>(result);
        }

        /// <summary>
        /// Gets chords contained in the specified <see cref="MidiFile"/>. More info in the
        /// <see href="xref:a_getting_objects#getchords">Getting objects: GetChords</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for chords.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Collection of chords contained in <paramref name="file"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessChords(MidiFile, Action{Chord}, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)"/>
        /// <seealso cref="ProcessChords(MidiFile, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)"/>
        /// <seealso cref="RemoveChords(MidiFile, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="RemoveChords(MidiFile, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<Chord> GetChords(
            this MidiFile file,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().GetChords(settings, noteDetectionSettings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Creates chords from notes.
        /// </summary>
        /// <param name="notes">Notes to create chords from.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Collection of chords made up from <paramref name="notes"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
        public static IEnumerable<Chord> GetChords(
            this IEnumerable<Note> notes,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            return new SortedTimedObjectsImmutableCollection<Chord>(notes.GetChordsAndNotesAndTimedEventsLazy(settings).OfType<Chord>().ToArray());
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_processing_objects#processchords">Processing objects: ProcessChords</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ChordProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with chords but dedicated methods of the <see cref="ChordsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(EventsCollection, ObjectType, Action{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessChords(
            this EventsCollection eventsCollection,
            Action<Chord> action,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);

            return eventsCollection.ProcessChords(action, chord => true, settings, noteDetectionSettings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_processing_objects#processchords">Processing objects: ProcessChords</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to process.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ChordProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with chords but dedicated methods of the <see cref="ChordsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
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
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(EventsCollection, ObjectType, Action{ITimedObject}, Predicate{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessChords(
            this EventsCollection eventsCollection,
            Action<Chord> action,
            Predicate<Chord> match,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return new[] { eventsCollection }.ProcessChordsInternal(
                action,
                match,
                settings,
                noteDetectionSettings,
                timedEventDetectionSettings,
                hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_processing_objects#processchords">Processing objects: ProcessChords</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ChordProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with chords but dedicated methods of the <see cref="ChordsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(TrackChunk, ObjectType, Action{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessChords(
            this TrackChunk trackChunk,
            Action<Chord> action,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunk.ProcessChords(action, chord => true, settings, noteDetectionSettings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_processing_objects#processchords">Processing objects: ProcessChords</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to process.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ChordProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with chords but dedicated methods of the <see cref="ChordsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
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
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(TrackChunk, ObjectType, Action{ITimedObject}, Predicate{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessChords(
            this TrackChunk trackChunk,
            Action<Chord> action,
            Predicate<Chord> match,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunk.Events.ProcessChords(action, match, settings, noteDetectionSettings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the collection of
        /// <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_processing_objects#processchords">Processing objects: ProcessChords</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ChordProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with chords but dedicated methods of the <see cref="ChordsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(IEnumerable{TrackChunk}, ObjectType, Action{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessChords(
            this IEnumerable<TrackChunk> trackChunks,
            Action<Chord> action,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunks.ProcessChords(action, chord => true, settings, noteDetectionSettings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the collection of
        /// <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_processing_objects#processchords">Processing objects: ProcessChords</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to process.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ChordProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with chords but dedicated methods of the <see cref="ChordsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
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
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(IEnumerable{TrackChunk}, ObjectType, Action{ITimedObject}, Predicate{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessChords(
            this IEnumerable<TrackChunk> trackChunks,
            Action<Chord> action,
            Predicate<Chord> match,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunks
                .Where(c => c != null)
                .Select(c => c.Events)
                .ProcessChordsInternal(
                    action,
                    match,
                    settings,
                    noteDetectionSettings,
                    timedEventDetectionSettings,
                    hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="MidiFile"/>.
        /// More info in the <see href="xref:a_processing_objects#processchords">Processing objects: ProcessChords</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ChordProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with chords but dedicated methods of the <see cref="ChordsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(MidiFile, ObjectType, Action{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessChords(
            this MidiFile file,
            Action<Chord> action,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);

            return file.ProcessChords(action, chord => true, settings, noteDetectionSettings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Chord"/> contained in the <see cref="MidiFile"/>.
        /// More info in the <see href="xref:a_processing_objects#processchords">Processing objects: ProcessChords</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for chords to process.</param>
        /// <param name="action">The action to perform on each <see cref="Chord"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to process.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ChordProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with chords but dedicated methods of the <see cref="ChordsManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
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
        /// <seealso cref="TimedObjectUtilities.ProcessObjects(MidiFile, ObjectType, Action{ITimedObject}, Predicate{ITimedObject}, ObjectDetectionSettings, ObjectProcessingHint)"/>
        public static int ProcessChords(
            this MidiFile file,
            Action<Chord> action,
            Predicate<Chord> match,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().ProcessChords(action, match, settings, noteDetectionSettings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Removes all chords from the <see cref="EventsCollection"/>. More info in the
        /// <see href="xref:a_removing_objects#removechords">Removing objects: RemoveChords</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for chords to remove.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(EventsCollection, ObjectType, ObjectDetectionSettings)"/>
        public static int RemoveChords(
            this EventsCollection eventsCollection,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return eventsCollection.RemoveChords(chord => true, settings, noteDetectionSettings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Removes chords that match the specified conditions from the <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_removing_objects#removechords">Removing objects: RemoveChords</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for chords to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to remove.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(EventsCollection, ObjectType, Predicate{ITimedObject}, ObjectDetectionSettings)"/>
        public static int RemoveChords(
            this EventsCollection eventsCollection,
            Predicate<Chord> match,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
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
                noteDetectionSettings,
                timedEventDetectionSettings,
                ChordProcessingHint.None);

            if (chordsToRemoveCount == 0)
                return 0;

            eventsCollection.RemoveTimedEvents(e => e.Event.Flag);
            return chordsToRemoveCount;
        }

        /// <summary>
        /// Removes all chords from the <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_removing_objects#removechords">Removing objects: RemoveChords</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for chords to remove.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(TrackChunk, ObjectType, ObjectDetectionSettings)"/>
        public static int RemoveChords(
            this TrackChunk trackChunk,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.RemoveChords(note => true, settings, noteDetectionSettings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Removes chords that match the specified conditions from the <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_removing_objects#removechords">Removing objects: RemoveChords</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for chords to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to remove.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(TrackChunk, ObjectType, Predicate{ITimedObject}, ObjectDetectionSettings)"/>
        public static int RemoveChords(
            this TrackChunk trackChunk,
            Predicate<Chord> match,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunk.Events.RemoveChords(match, settings, noteDetectionSettings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Removes all chords from the collection of <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_removing_objects#removechords">Removing objects: RemoveChords</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for chords to remove.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(IEnumerable{TrackChunk}, ObjectType, ObjectDetectionSettings)"/>
        public static int RemoveChords(
            this IEnumerable<TrackChunk> trackChunks,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return trackChunks.RemoveChords(note => true, settings, noteDetectionSettings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Removes chords that match the specified conditions from the collection of <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_removing_objects#removechords">Removing objects: RemoveChords</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for chords to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to remove.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(IEnumerable{TrackChunk}, ObjectType, Predicate{ITimedObject}, ObjectDetectionSettings)"/>
        public static int RemoveChords(
            this IEnumerable<TrackChunk> trackChunks,
            Predicate<Chord> match,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
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
                noteDetectionSettings,
                timedEventDetectionSettings,
                ChordProcessingHint.None);

            if (chordsToRemoveCount == 0)
                return 0;

            trackChunks.RemoveTimedEvents(e => e.Event.Flag);
            return chordsToRemoveCount;
        }

        /// <summary>
        /// Removes all chords from the <see cref="MidiFile"/>. More info in the
        /// <see href="xref:a_removing_objects#removechords">Removing objects: RemoveChords</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for chords to remove.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(MidiFile, ObjectType, ObjectDetectionSettings)"/>
        public static int RemoveChords(
            this MidiFile file,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.RemoveChords(chord => true, settings, noteDetectionSettings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Removes chords that match the specified conditions from the <see cref="MidiFile"/>.
        /// More info in the <see href="xref:a_removing_objects#removechords">Removing objects: RemoveChords</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for chords to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Chord"/> to remove.</param>
        /// <param name="settings">Settings according to which chords should be detected and built.</param>
        /// <param name="noteDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes for chords.</param>
        /// <returns>Count of removed chords.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(MidiFile, ObjectType, Predicate{ITimedObject}, ObjectDetectionSettings)"/>
        public static int RemoveChords(
            this MidiFile file,
            Predicate<Chord> match,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().RemoveChords(match, settings, noteDetectionSettings, timedEventDetectionSettings);
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

        internal static IEnumerable<TimedObjectAt<ITimedObject>> GetChordsAndNotesAndTimedEventsLazy(
            this IEnumerable<TimedObjectAt<TimedEvent>> timedEvents,
            ChordDetectionSettings settings,
            NoteDetectionSettings noteDetectionSettings,
            TimedEventDetectionSettings timedEventDetectionSettings)
        {
            settings = settings ?? new ChordDetectionSettings();
            noteDetectionSettings = noteDetectionSettings ?? new NoteDetectionSettings();
            timedEventDetectionSettings = timedEventDetectionSettings ?? new TimedEventDetectionSettings();

            var constructor = settings.Constructor;

            var timedObjects = new LinkedList<IObjectDescriptorIndexed>();
            var chordsDescriptors = new LinkedList<ChordDescriptorIndexed>();
            var chordsDescriptorsByChannel = new LinkedListNode<ChordDescriptorIndexed>[FourBitNumber.MaxValue + 1];

            var notesTolerance = settings.NotesTolerance;

            foreach (var timedObjectTuple in timedEvents.GetNotesAndTimedEventsLazy(noteDetectionSettings ?? new NoteDetectionSettings()))
            {
                var timedObject = timedObjectTuple.Object;

                var timedEvent = timedObject as TimedEvent;
                if (timedEvent != null)
                {
                    if (timedObjects.Count == 0)
                        yield return new TimedObjectAt<ITimedObject>(timedObject, timedObjectTuple.AtIndex);
                    else
                        timedObjects.AddLast(new TimedEventDescriptorIndexed(timedEvent, timedObjectTuple.AtIndex));

                    continue;
                }

                var note = (Note)timedObject;
                var chordDescriptorNode = chordsDescriptorsByChannel[note.Channel];

                if (timedObjects.Count == 0 || chordDescriptorNode == null || chordDescriptorNode.List == null)
                {
                    CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, timedObjects, note, timedObjectTuple.AtIndex, settings);
                }
                else
                {
                    var chordDescriptor = chordDescriptorNode.Value;
                    if (CanNoteBeAddedToChord(chordDescriptor, note, notesTolerance, timedObjectTuple.AtIndex))
                    {
                        var noteNode = timedObjects.AddLast(new NoteDescriptorIndexed(note, timedObjectTuple.AtIndex, false) { ChordDescriptor = chordDescriptor });
                        chordDescriptor.NotesNodes.Add(noteNode);
                    }
                    else
                    {
                        chordDescriptor.IsSealed = true;

                        if (chordDescriptorNode.Previous == null)
                        {
                            foreach (var timedObjectX in GetTimedObjects(chordDescriptorNode, chordsDescriptors, timedObjects, true, constructor))
                            {
                                yield return timedObjectX;
                            }
                        }

                        CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, timedObjects, note, timedObjectTuple.AtIndex, settings);
                    }
                }
            }

            foreach (var timedObject in GetTimedObjects(chordsDescriptors.First, chordsDescriptors, timedObjects, false, constructor))
            {
                yield return timedObject;
            }
        }

        internal static IEnumerable<ITimedObject> GetChordsAndNotesAndTimedEventsLazy(
            this IEnumerable<TimedEvent> timedEvents,
            ChordDetectionSettings settings,
            NoteDetectionSettings noteDetectionSettings,
            TimedEventDetectionSettings timedEventDetectionSettings)
        {
            settings = settings ?? new ChordDetectionSettings();

            return timedEvents
                .GetNotesAndTimedEventsLazy(noteDetectionSettings ?? new NoteDetectionSettings())
                .GetChordsAndNotesAndTimedEventsLazy(settings);
        }

        internal static IEnumerable<ITimedObject> GetChordsAndNotesAndTimedEventsLazy(
            this IEnumerable<ITimedObject> notesAndTimedEvents,
            ChordDetectionSettings settings)
        {
            return notesAndTimedEvents.GetChordsAndNotesAndTimedEventsLazy(settings, false);
        }

        internal static IEnumerable<ITimedObject> GetChordsAndNotesAndTimedEventsLazy(
            this IEnumerable<ITimedObject> notesAndTimedEvents,
            ChordDetectionSettings settings,
            bool chordsAllowed)
        {
            settings = settings ?? new ChordDetectionSettings();
            var constructor = settings.Constructor;

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
                        var noteNode = timedObjects.AddLast(new NoteDescriptor(note, false) { ChordDescriptor = chordDescriptor });
                        chordDescriptor.NotesNodes.Add(noteNode);
                    }
                    else
                    {
                        chordDescriptor.IsSealed = true;

                        if (chordDescriptorNode.Previous == null)
                        {
                            foreach (var timedObjectX in GetTimedObjects(chordDescriptorNode, chordsDescriptors, timedObjects, true, constructor))
                            {
                                yield return timedObjectX;
                            }
                        }

                        CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, timedObjects, note, settings);
                    }
                }
            }

            foreach (var timedObject in GetTimedObjects(chordsDescriptors.First, chordsDescriptors, timedObjects, false, constructor))
            {
                yield return timedObject;
            }
        }

        internal static int ProcessChordsInternal(
            this IEnumerable<EventsCollection> eventsCollectionsIn,
            Action<Chord> action,
            Predicate<Chord> match,
            ChordDetectionSettings settings,
            NoteDetectionSettings noteDetectionSettings,
            TimedEventDetectionSettings timedEventDetectionSettings,
            ChordProcessingHint hint)
        {
            settings = settings ?? new ChordDetectionSettings();
            noteDetectionSettings = noteDetectionSettings ?? new NoteDetectionSettings();
            timedEventDetectionSettings = timedEventDetectionSettings ?? new TimedEventDetectionSettings();

            var eventsCollections = eventsCollectionsIn.Where(c => c != null).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            var iMatched = 0;

            var timeOrLengthChanged = false;
            var notesCollectionChanged = false;
            var noteTimeOrLengthChanged = false;

            var timeOrLengthCanBeChanged = hint.HasFlag(ChordProcessingHint.TimeOrLengthCanBeChanged);
            var notesCollectionCanBeChanged = hint.HasFlag(ChordProcessingHint.NotesCollectionCanBeChanged);
            var noteTimeOrLengthCanBeChanged = hint.HasFlag(ChordProcessingHint.NoteTimeOrLengthCanBeChanged);

            var collectedTimedEvents = timeOrLengthCanBeChanged || notesCollectionCanBeChanged || noteTimeOrLengthCanBeChanged
                ? new List<TimedObjectAt<TimedEvent>>(eventsCount)
                : null;

            var chordsBuilder = new ChordsBuilder(settings, noteDetectionSettings);
            var chords = chordsBuilder.GetChordsLazy(eventsCollections.GetTimedEventsLazy(eventsCount, timedEventDetectionSettings, false), collectedTimedEvents != null, collectedTimedEvents);

            foreach (var chordAt in chords)
            {
                var chord = chordAt.Object;
                if (!match(chord))
                    continue;

                long time;
                long length;
                chord.GetTimeAndLength(out time, out length);

                var notes = notesCollectionCanBeChanged || noteTimeOrLengthCanBeChanged
                    ? chord.Notes.ToArray()
                    : null;

                var notesTimes = noteTimeOrLengthCanBeChanged
                    ? notes.ToDictionary(n => n, n => n.Time)
                    : null;
                var notesLengths = noteTimeOrLengthCanBeChanged
                    ? notes.ToDictionary(n => n, n => n.Length)
                    : null;

                action(chord);

                long newTime;
                long newLength;
                chord.GetTimeAndLength(out newTime, out newLength);
                timeOrLengthChanged |=
                    newTime != time ||
                    newLength != length;

                var addedNotes = notesCollectionCanBeChanged ? chord.Notes.Except(notes).ToArray() : null;
                var removedNotes = notesCollectionCanBeChanged ? notes.Except(chord.Notes).ToArray() : null;
                notesCollectionChanged |=
                    addedNotes?.Length > 0 ||
                    removedNotes?.Length > 0;

                var savedNotes = noteTimeOrLengthCanBeChanged
                    ? (notesCollectionCanBeChanged ? (IEnumerable<Note>)notes.Intersect(chord.Notes).ToArray() : chord.Notes)
                    : null;
                noteTimeOrLengthChanged |=
                    savedNotes?.Any(n => n.Time != notesTimes[n]) == true ||
                    savedNotes?.Any(n => n.Length != notesLengths[n]) == true;

                if (notesCollectionChanged)
                {
                    foreach (var note in addedNotes)
                    {
                        collectedTimedEvents?.Add(new TimedObjectAt<TimedEvent>(
                            note.TimedNoteOnEvent,
                            chordAt.AtIndex));
                        collectedTimedEvents?.Add(new TimedObjectAt<TimedEvent>(
                            note.TimedNoteOffEvent,
                            chordAt.AtIndex));
                    }

                    foreach (var note in removedNotes)
                    {
                        note.TimedNoteOnEvent.Event.Flag = note.TimedNoteOffEvent.Event.Flag = true;
                    }
                }

                iMatched++;
            }

            if ((!timeOrLengthCanBeChanged || !timeOrLengthChanged) &&
                (!notesCollectionCanBeChanged || !notesCollectionChanged) &&
                (!noteTimeOrLengthCanBeChanged || !noteTimeOrLengthChanged))
                return iMatched;

            eventsCollections.SortAndUpdateEvents(collectedTimedEvents);

            return iMatched;
        }

        private static IEnumerable<ITimedObject> GetTimedObjects(
            LinkedListNode<ChordDescriptor> startChordDescriptorNode,
            LinkedList<ChordDescriptor> chordsDescriptors,
            LinkedList<IObjectDescriptor> timedObjects,
            bool getSealedOnly,
            Func<ChordData, Chord> constructor)
        {
            for (var chordDescriptorNode = startChordDescriptorNode; chordDescriptorNode != null;)
            {
                var chordDescriptor = chordDescriptorNode.Value;
                if (getSealedOnly && !chordDescriptor.IsSealed)
                    break;

                if (chordDescriptor.IsCompleted)
                {
                    foreach (var noteNode in chordDescriptor.NotesNodes)
                    {
                        timedObjects.Remove(noteNode);
                    }

                    var notes = chordDescriptor.NotesNodes.Select(n => (Note)((NoteDescriptor)n.Value).TimedObject);
                    yield return constructor == null
                        ? new Chord(notes)
                        : constructor(new ChordData(notes.ToArray()));
                }

                for (var node = timedObjects.First; node != null && (!node.Value.ChordStart || node.Value.ChordDescriptor?.IsCompleted == false);)
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

        private static IEnumerable<TimedObjectAt<ITimedObject>> GetTimedObjects(
            LinkedListNode<ChordDescriptorIndexed> startChordDescriptorNode,
            LinkedList<ChordDescriptorIndexed> chordsDescriptors,
            LinkedList<IObjectDescriptorIndexed> timedObjects,
            bool getSealedOnly,
            Func<ChordData, Chord> constructor)
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

                    for (var i = 0; i < notesCount; i++)
                    {
                        timedObjects.Remove(chordDescriptor.NotesNodes[i]);
                        notes[i] = (Note)chordDescriptor.NotesNodes[i].Value.TimedObject;
                    }

                    yield return new TimedObjectAt<ITimedObject>(
                        constructor == null ? new Chord(notes) : constructor(new ChordData(notes)),
                        chordDescriptor.NotesNodes[0].Value.IndexedTimedObject.AtIndex);
                }

                for (var node = timedObjects.First; node != null && (!chordDescriptor.IsCompleted || !node.Value.ChordStart);)
                {
                    var timedObject = node.Value.IndexedTimedObject;
                    yield return new TimedObjectAt<ITimedObject>(timedObject.Object, timedObject.AtIndex);

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
            var noteDescriptor = new NoteDescriptor(note, true);
            var noteNode = timedObjects.AddLast(noteDescriptor);
            var chordDescriptor = new ChordDescriptor(note.Time, noteNode, settings.NotesMinCount);
            noteDescriptor.ChordDescriptor = chordDescriptor;
            chordsDescriptorsByChannel[note.Channel] = chordsDescriptors.AddLast(chordDescriptor);
        }

        private static void CreateChordDescriptor(
            LinkedList<ChordDescriptorIndexed> chordsDescriptors,
            LinkedListNode<ChordDescriptorIndexed>[] chordsDescriptorsByChannel,
            LinkedList<IObjectDescriptorIndexed> timedObjects,
            Note note,
            int noteOnIndex,
            ChordDetectionSettings settings)
        {
            var noteDescriptor = new NoteDescriptorIndexed(note, noteOnIndex, true);
            var noteNode = timedObjects.AddLast(noteDescriptor);
            var chordDescriptor = new ChordDescriptorIndexed(note.Time, noteNode, settings.NotesMinCount) { EventsCollectionIndex = noteOnIndex };
            noteDescriptor.ChordDescriptor = chordDescriptor;
            chordsDescriptorsByChannel[note.Channel] = chordsDescriptors.AddLast(chordDescriptor);
        }

        private static bool CanNoteBeAddedToChord(ChordDescriptor chordDescriptor, Note note, long notesTolerance)
        {
            return note.Time - chordDescriptor.Time <= notesTolerance;
        }

        private static bool CanNoteBeAddedToChord(ChordDescriptorIndexed chordDescriptor, Note note, long notesTolerance, int eventsCollectionIndex)
        {
            return note.Time - chordDescriptor.Time <= notesTolerance && chordDescriptor.EventsCollectionIndex == eventsCollectionIndex;
        }

        #endregion
    }
}
