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

        private abstract class NoteOnsHolderBase<TDescriptor> where TDescriptor : IObjectDescriptor
        {
            private const int DefaultCapacity = 2;

            private readonly NoteStartDetectionPolicy _noteStartDetectionPolicy;

            private readonly Stack<LinkedListNode<TDescriptor>> _nodesStack;
            private readonly Queue<LinkedListNode<TDescriptor>> _nodesQueue;

            protected NoteOnsHolderBase(NoteStartDetectionPolicy noteStartDetectionPolicy)
            {
                switch (noteStartDetectionPolicy)
                {
                    case NoteStartDetectionPolicy.LastNoteOn:
                        _nodesStack = new Stack<LinkedListNode<TDescriptor>>(DefaultCapacity);
                        break;
                    case NoteStartDetectionPolicy.FirstNoteOn:
                        _nodesQueue = new Queue<LinkedListNode<TDescriptor>>(DefaultCapacity);
                        break;
                }

                _noteStartDetectionPolicy = noteStartDetectionPolicy;
            }

            public int Count
            {
                get
                {
                    switch (_noteStartDetectionPolicy)
                    {
                        case NoteStartDetectionPolicy.LastNoteOn:
                            return _nodesStack.Count;
                        case NoteStartDetectionPolicy.FirstNoteOn:
                            return _nodesQueue.Count;
                    }

                    return -1;
                }
            }

            public void Add(LinkedListNode<TDescriptor> noteOnNode)
            {
                switch (_noteStartDetectionPolicy)
                {
                    case NoteStartDetectionPolicy.LastNoteOn:
                        _nodesStack.Push(noteOnNode);
                        break;
                    case NoteStartDetectionPolicy.FirstNoteOn:
                        _nodesQueue.Enqueue(noteOnNode);
                        break;
                }
            }

            public LinkedListNode<TDescriptor> GetNext()
            {
                switch (_noteStartDetectionPolicy)
                {
                    case NoteStartDetectionPolicy.LastNoteOn:
                        return _nodesStack.Pop();
                    case NoteStartDetectionPolicy.FirstNoteOn:
                        return _nodesQueue.Dequeue();
                }

                return null;
            }
        }

        private sealed class NoteOnsHolder : NoteOnsHolderBase<IObjectDescriptor>
        {
            public NoteOnsHolder(NoteStartDetectionPolicy noteStartDetectionPolicy)
                : base(noteStartDetectionPolicy)
            {
            }
        }

        private sealed class NoteOnsHolderIndexed : NoteOnsHolderBase<IObjectDescriptorIndexed>
        {
            public NoteOnsHolderIndexed(NoteStartDetectionPolicy noteStartDetectionPolicy)
                : base(noteStartDetectionPolicy)
            {
            }
        }

        private interface IObjectDescriptor
        {
            bool IsCompleted { get; }

            ITimedObject GetObject(Func<NoteData, Note> constructor);
        }

        private interface IObjectDescriptorIndexed : IObjectDescriptor
        {
            TimedObjectAt<ITimedObject> GetIndexedObject(Func<NoteData, Note> constructor);
        }

        private class NoteDescriptor : IObjectDescriptor
        {
            private TimedEvent _noteOnTimedEvent;

            public NoteDescriptor(TimedEvent noteOnTimedEvent)
            {
                _noteOnTimedEvent = noteOnTimedEvent;
            }

            public TimedEvent NoteOffTimedEvent { get; set; }

            public bool IsCompleted => NoteOffTimedEvent != null;

            public ITimedObject GetObject(Func<NoteData, Note> constructor)
            {
                return IsCompleted
                    ? (constructor == null
                        ? new Note(_noteOnTimedEvent, NoteOffTimedEvent, false)
                        : constructor(new NoteData(_noteOnTimedEvent, NoteOffTimedEvent)))
                    : (ITimedObject)_noteOnTimedEvent;
            }
        }

        private class CompleteObjectDescriptor : IObjectDescriptor
        {
            private readonly ITimedObject _timedObject;

            public CompleteObjectDescriptor(ITimedObject timedObject)
            {
                _timedObject = timedObject;
            }

            public bool IsCompleted { get; } = true;

            public ITimedObject GetObject(Func<NoteData, Note> constructor)
            {
                return _timedObject;
            }
        }

        private sealed class NoteDescriptorIndexed : NoteDescriptor, IObjectDescriptorIndexed
        {
            private readonly int _index;

            public NoteDescriptorIndexed(TimedEvent noteOnTimedEvent, int index)
                : base(noteOnTimedEvent)
            {
                _index = index;
            }

            public TimedObjectAt<ITimedObject> GetIndexedObject(Func<NoteData, Note> constructor)
            {
                return new TimedObjectAt<ITimedObject>(GetObject(constructor), _index);
            }
        }

        private class TimedEventDescriptor : IObjectDescriptor
        {
            private TimedEvent _timedEvent;

            public TimedEventDescriptor(TimedEvent timedEvent)
            {
                _timedEvent = timedEvent;
            }

            public bool IsCompleted { get; } = true;

            public ITimedObject GetObject(Func<NoteData, Note> constructor)
            {
                return _timedEvent;
            }
        }

        private sealed class TimedEventDescriptorIndexed : TimedEventDescriptor, IObjectDescriptorIndexed
        {
            private readonly int _index;

            public TimedEventDescriptorIndexed(TimedEvent timedEvent, int index)
                : base(timedEvent)
            {
                _index = index;
            }

            public TimedObjectAt<ITimedObject> GetIndexedObject(Func<NoteData, Note> constructor)
            {
                return new TimedObjectAt<ITimedObject>(GetObject(constructor), _index);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the <see cref="TimedObjectsManager{Note}"/> initializing it with the
        /// specified events collection. More info in the <see href="xref:a_managers">Objects managers</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds notes to manage.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="comparer">Comparer that will be used to order objects on enumerating and saving objects
        /// back to the <paramref name="eventsCollection"/> via <see cref="TimedObjectsManager{TObject}.SaveChanges"/>
        /// or <see cref="TimedObjectsManager{TObject}.Dispose()"/>.</param>
        /// <returns>An instance of the <see cref="TimedObjectsManager{Note}"/> that can be used to manage
        /// notes represented by the <paramref name="eventsCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static TimedObjectsManager<Note> ManageNotes(
            this EventsCollection eventsCollection,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            TimedObjectsComparer comparer = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return new TimedObjectsManager<Note>(
                eventsCollection,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = settings,
                    TimedEventDetectionSettings = timedEventDetectionSettings
                },
                comparer);
        }

        /// <summary>
        /// Creates an instance of the <see cref="TimedObjectsManager{Note}"/> initializing it with the
        /// events collection of the specified track chunk. More info in the
        /// <see href="xref:a_managers">Objects managers</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> that holds notes to manage.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="comparer">Comparer that will be used to order objects on enumerating and saving objects
        /// back to the <paramref name="trackChunk"/> via <see cref="TimedObjectsManager{TObject}.SaveChanges"/>
        /// or <see cref="TimedObjectsManager{TObject}.Dispose()"/>.</param>
        /// <returns>An instance of the <see cref="TimedObjectsManager{Note}"/> that can be used to manage
        /// notes represented by the <paramref name="trackChunk"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static TimedObjectsManager<Note> ManageNotes(
            this TrackChunk trackChunk,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            TimedObjectsComparer comparer = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.ManageNotes(settings, timedEventDetectionSettings, comparer);
        }

        /// <summary>
        /// Gets notes contained in the specified collection of <see cref="MidiEvent"/>. More info in the
        /// <see href="xref:a_getting_objects#getnotes">Getting objects: GetNotes</see> article.
        /// </summary>
        /// <param name="midiEvents">Collection of <see cref="MidiEvent"/> to search for notes.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Collection of notes contained in <paramref name="midiEvents"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is <c>null</c>.</exception>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<Note> GetNotes(
            this IEnumerable<MidiEvent> midiEvents,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            var result = new List<Note>();
            var notesBuilder = new NotesBuilder(settings);

            var notes = notesBuilder.GetNotesLazy(midiEvents.GetTimedEventsLazy(timedEventDetectionSettings));

            result.AddRange(notes);
            return new SortedTimedObjectsImmutableCollection<Note>(result);
        }

        /// <summary>
        /// Gets notes contained in the specified <see cref="EventsCollection"/>. More info in the
        /// <see href="xref:a_getting_objects#getnotes">Getting objects: GetNotes</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Collection of notes contained in <paramref name="eventsCollection"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessNotes(EventsCollection, Action{Note}, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)"/>
        /// <seealso cref="ProcessNotes(EventsCollection, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)"/>
        /// <seealso cref="RemoveNotes(EventsCollection, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="RemoveNotes(EventsCollection, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<Note> GetNotes(
            this EventsCollection eventsCollection,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            var result = new List<Note>();
            var notesBuilder = new NotesBuilder(settings);

            var notes = notesBuilder.GetNotesLazy(eventsCollection.GetTimedEventsLazy(timedEventDetectionSettings));

            result.AddRange(notes);
            return new SortedTimedObjectsImmutableCollection<Note>(result);
        }

        /// <summary>
        /// Gets notes contained in the specified <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_getting_objects#getnotes">Getting objects: GetNotes</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Collection of notes contained in <paramref name="trackChunk"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessNotes(TrackChunk, Action{Note}, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)"/>
        /// <seealso cref="ProcessNotes(TrackChunk, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)"/>
        /// <seealso cref="RemoveNotes(TrackChunk, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="RemoveNotes(TrackChunk, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<Note> GetNotes(
            this TrackChunk trackChunk,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.GetNotes(settings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Gets notes contained in the specified collection of <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_getting_objects#getnotes">Getting objects: GetNotes</see> article.
        /// </summary>
        /// <param name="trackChunks">Track chunks to search for notes.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Collection of notes contained in <paramref name="trackChunks"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessNotes(IEnumerable{TrackChunk}, Action{Note}, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)"/>
        /// <seealso cref="ProcessNotes(IEnumerable{TrackChunk}, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)"/>
        /// <seealso cref="RemoveNotes(IEnumerable{TrackChunk}, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="RemoveNotes(IEnumerable{TrackChunk}, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<Note> GetNotes(
            this IEnumerable<TrackChunk> trackChunks,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var eventsCollections = trackChunks.Select(c => c.Events).ToArray();

            switch (eventsCollections.Length)
            {
                case 0: return new Note[0];
                case 1: return eventsCollections[0].GetNotes(settings, timedEventDetectionSettings);
            }

            var eventsCount = eventsCollections.Sum(e => e.Count);
            var result = new List<Note>(eventsCount / 3);
            var notesBuilder = new NotesBuilder(settings);

            var notes = notesBuilder.GetNotesLazy(eventsCollections.GetTimedEventsLazy(eventsCount, timedEventDetectionSettings));

            result.AddRange(notes.Select(n => n.Object));
            return new SortedTimedObjectsImmutableCollection<Note>(result);
        }

        /// <summary>
        /// Gets notes contained in the specified <see cref="MidiFile"/>. More info in the
        /// <see href="xref:a_getting_objects#getnotes">Getting objects: GetNotes</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Collection of notes contained in <paramref name="file"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        /// <seealso cref="ProcessNotes(MidiFile, Action{Note}, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)"/>
        /// <seealso cref="ProcessNotes(MidiFile, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)"/>
        /// <seealso cref="RemoveNotes(MidiFile, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="RemoveNotes(MidiFile, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings)"/>
        /// <seealso cref="GetObjectsUtilities"/>
        public static ICollection<Note> GetNotes(
            this MidiFile file,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().GetNotes(settings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_processing_objects#processnotes">Processing objects: ProcessNotes</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="NoteProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with notes but dedicated methods of the <see cref="NotesManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(
            this EventsCollection eventsCollection,
            Action<Note> action,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);

            return eventsCollection.ProcessNotes(action, note => true, settings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_processing_objects#processnotes">Processing objects: ProcessNotes</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="NoteProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with notes but dedicated methods of the <see cref="NotesManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(
            this EventsCollection eventsCollection,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return new[] { eventsCollection }.ProcessNotesInternal(action, match, settings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_processing_objects#processnotes">Processing objects: ProcessNotes</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="NoteProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with notes but dedicated methods of the <see cref="NotesManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(
            this TrackChunk trackChunk,
            Action<Note> action,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunk.ProcessNotes(action, note => true, settings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_processing_objects#processnotes">Processing objects: ProcessNotes</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="NoteProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with notes but dedicated methods of the <see cref="NotesManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(
            this TrackChunk trackChunk,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunk.Events.ProcessNotes(action, match, settings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the collection of
        /// <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_processing_objects#processnotes">Processing objects: ProcessNotes</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="NoteProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with notes but dedicated methods of the <see cref="NotesManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(
            this IEnumerable<TrackChunk> trackChunks,
            Action<Note> action,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunks.ProcessNotes(action, note => true, settings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the collection of
        /// <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_processing_objects#processnotes">Processing objects: ProcessNotes</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="NoteProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with notes but dedicated methods of the <see cref="NotesManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(
            this IEnumerable<TrackChunk> trackChunks,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunks
                .Where(c => c != null)
                .Select(c => c.Events)
                .ProcessNotesInternal(action, match, settings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="MidiFile"/>.
        /// More info in the <see href="xref:a_processing_objects#processnotes">Processing objects: ProcessNotes</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="NoteProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with notes but dedicated methods of the <see cref="NotesManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(
            this MidiFile file,
            Action<Note> action,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);

            return file.ProcessNotes(action, note => true, settings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="MidiFile"/>.
        /// More info in the <see href="xref:a_processing_objects#processnotes">Processing objects: ProcessNotes</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="NoteProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with notes but dedicated methods of the <see cref="NotesManagingUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(
            this MidiFile file,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().ProcessNotes(action, match, settings, timedEventDetectionSettings, hint);
        }

        /// <summary>
        /// Removes all notes from the <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_removing_objects#removenotes">Removing objects: RemoveNotes</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes to remove.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Count of removed notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(EventsCollection, ObjectType, ObjectDetectionSettings)"/>
        public static int RemoveNotes(
            this EventsCollection eventsCollection,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return eventsCollection.RemoveNotes(note => true, settings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Removes notes that match the specified conditions from the <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_removing_objects#removenotes">Removing objects: RemoveNotes</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Count of removed notes.</returns>
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
        public static int RemoveNotes(
            this EventsCollection eventsCollection,
            Predicate<Note> match,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(match), match);

            var notesToRemoveCount = eventsCollection.ProcessNotes(
                n => n.TimedNoteOnEvent.Event.Flag = n.TimedNoteOffEvent.Event.Flag = true,
                match,
                settings,
                timedEventDetectionSettings,
                NoteProcessingHint.None);

            if (notesToRemoveCount == 0)
                return 0;

            eventsCollection.RemoveTimedEvents(e => e.Event.Flag);
            return notesToRemoveCount;
        }

        /// <summary>
        /// Removes all notes from the <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_removing_objects#removenotes">Removing objects: RemoveNotes</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes to remove.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Count of removed notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(TrackChunk, ObjectType, ObjectDetectionSettings)"/>
        public static int RemoveNotes(
            this TrackChunk trackChunk,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.RemoveNotes(note => true, settings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Removes notes that match the specified conditions from the <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_removing_objects#removenotes">Removing objects: RemoveNotes</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Count of removed notes.</returns>
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
        public static int RemoveNotes(
            this TrackChunk trackChunk,
            Predicate<Note> match,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunk.Events.RemoveNotes(match, settings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Removes all notes from the collection of <see cref="EventsCollection"/>.
        /// More info in the <see href="xref:a_removing_objects#removenotes">Removing objects: RemoveNotes</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for notes to remove.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Count of removed notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(IEnumerable{TrackChunk}, ObjectType, ObjectDetectionSettings)"/>
        public static int RemoveNotes(
            this IEnumerable<TrackChunk> trackChunks,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return trackChunks.RemoveNotes(note => true, settings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Removes notes that match the specified conditions from the collection of <see cref="TrackChunk"/>.
        /// More info in the <see href="xref:a_removing_objects#removenotes">Removing objects: RemoveNotes</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Count of removed notes.</returns>
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
        public static int RemoveNotes(
            this IEnumerable<TrackChunk> trackChunks,
            Predicate<Note> match,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(match), match);

            var notesToRemoveCount = trackChunks.ProcessNotes(
                n => n.TimedNoteOnEvent.Event.Flag = n.TimedNoteOffEvent.Event.Flag = true,
                match,
                settings,
                timedEventDetectionSettings,
                NoteProcessingHint.None);

            if (notesToRemoveCount == 0)
                return 0;

            trackChunks.RemoveTimedEvents(e => e.Event.Flag);
            return notesToRemoveCount;
        }

        /// <summary>
        /// Removes all notes from the <see cref="MidiFile"/>.
        /// More info in the <see href="xref:a_removing_objects#removenotes">Removing objects: RemoveNotes</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes to remove.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Count of removed notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        /// <seealso cref="TimedObjectUtilities.RemoveObjects(MidiFile, ObjectType, ObjectDetectionSettings)"/>
        public static int RemoveNotes(
            this MidiFile file,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.RemoveNotes(note => true, settings, timedEventDetectionSettings);
        }

        /// <summary>
        /// Removes notes that match the specified conditions from the <see cref="MidiFile"/>.
        /// More info in the <see href="xref:a_removing_objects#removenotes">Removing objects: RemoveNotes</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="timedEventDetectionSettings">Settings according to which timed events should be detected
        /// and built to construct notes.</param>
        /// <returns>Count of removed notes.</returns>
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
        public static int RemoveNotes(
            this MidiFile file,
            Predicate<Note> match,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().RemoveNotes(match, settings, timedEventDetectionSettings);
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

        internal static int ProcessNotesInternal(
            this IEnumerable<EventsCollection> eventsCollectionsIn,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings noteDetectionSettings,
            TimedEventDetectionSettings timedEventDetectionSettings,
            NoteProcessingHint hint)
        {
            var eventsCollections = eventsCollectionsIn.Where(c => c != null).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            var iMatched = 0;

            var timeOrLengthChanged = false;
            var timeOrLengthCanBeChanged = hint.HasFlag(NoteProcessingHint.TimeOrLengthCanBeChanged);
            var collectedTimedEvents = timeOrLengthCanBeChanged ? new List<TimedObjectAt<TimedEvent>>(eventsCount) : null;

            var notesBuilder = new NotesBuilder(noteDetectionSettings);
            var notes = notesBuilder.GetNotesLazy(eventsCollections.GetTimedEventsLazy(eventsCount, timedEventDetectionSettings, false), collectedTimedEvents != null, collectedTimedEvents);

            foreach (var noteAt in notes)
            {
                var note = noteAt.Object;
                if (match(note))
                {
                    var startTime = note.TimedNoteOnEvent.Time;
                    var endTime = note.TimedNoteOffEvent.Time;

                    action(note);

                    timeOrLengthChanged |= note.TimedNoteOnEvent.Time != startTime || note.TimedNoteOffEvent.Time != endTime;

                    iMatched++;
                }
            }

            if (timeOrLengthCanBeChanged && timeOrLengthChanged)
                eventsCollections.SortAndUpdateEvents(collectedTimedEvents);

            return iMatched;
        }

        internal static IEnumerable<TimedObjectAt<ITimedObject>> GetNotesAndTimedEventsLazy(
            this IEnumerable<TimedObjectAt<TimedEvent>> timedEvents,
            NoteDetectionSettings settings)
        {
            var objectsDescriptors = new LinkedList<IObjectDescriptorIndexed>();
            var notesDescriptorsNodes = new Dictionary<Tuple<int, int>, NoteOnsHolderIndexed>();

            var constructor = settings?.Constructor;

            foreach (var timedEventTuple in timedEvents)
            {
                var timedEvent = timedEventTuple.Object;

                switch (timedEvent.Event.EventType)
                {
                    case MidiEventType.NoteOn:
                        {
                            var noteId = GetNoteEventId((NoteOnEvent)timedEvent.Event);
                            var noteFullId = Tuple.Create(noteId, timedEventTuple.AtIndex);
                            var node = objectsDescriptors.AddLast(new NoteDescriptorIndexed(timedEvent, timedEventTuple.AtIndex));

                            NoteOnsHolderIndexed noteOnsHolder;
                            if (!notesDescriptorsNodes.TryGetValue(noteFullId, out noteOnsHolder))
                                notesDescriptorsNodes.Add(noteFullId, noteOnsHolder = new NoteOnsHolderIndexed(settings.NoteStartDetectionPolicy));

                            noteOnsHolder.Add(node);
                        }
                        break;
                    case MidiEventType.NoteOff:
                        {
                            var noteId = GetNoteEventId((NoteOffEvent)timedEvent.Event);
                            var noteFullId = Tuple.Create(noteId, timedEventTuple.AtIndex);

                            NoteOnsHolderIndexed noteOnsHolder;
                            LinkedListNode<IObjectDescriptorIndexed> node;

                            if (!notesDescriptorsNodes.TryGetValue(noteFullId, out noteOnsHolder) || noteOnsHolder.Count == 0 || (node = noteOnsHolder.GetNext()).List == null)
                            {
                                objectsDescriptors.AddLast(new TimedEventDescriptorIndexed(timedEvent, timedEventTuple.AtIndex));
                                break;
                            }

                            var noteDescriptorIndexed = (NoteDescriptorIndexed)node.Value;
                            noteDescriptorIndexed.NoteOffTimedEvent = timedEvent;

                            var previousNode = node.Previous;
                            if (previousNode != null)
                                break;

                            for (var n = node; n != null;)
                            {
                                if (!n.Value.IsCompleted)
                                    break;

                                yield return n.Value.GetIndexedObject(constructor);

                                var next = n.Next;
                                objectsDescriptors.Remove(n);
                                n = next;
                            }
                        }
                        break;
                    default:
                        {
                            if (objectsDescriptors.Count == 0)
                                yield return new TimedObjectAt<ITimedObject>(timedEvent, timedEventTuple.AtIndex);
                            else
                                objectsDescriptors.AddLast(new TimedEventDescriptorIndexed(timedEvent, timedEventTuple.AtIndex));
                        }
                        break;
                }
            }

            foreach (var objectDescriptor in objectsDescriptors)
            {
                yield return objectDescriptor.GetIndexedObject(constructor);
            }
        }

        internal static IEnumerable<ITimedObject> GetNotesAndTimedEventsLazy(
            this IEnumerable<TimedEvent> timedEvents,
            NoteDetectionSettings settings)
        {
            return GetNotesAndTimedEventsLazy(timedEvents, settings, false);
        }

        internal static IEnumerable<ITimedObject> GetNotesAndTimedEventsLazy(
            this IEnumerable<ITimedObject> timedObjects,
            NoteDetectionSettings settings,
            bool completeObjectsAllowed)
        {
            settings = settings ?? new NoteDetectionSettings();
            var constructor = settings?.Constructor;

            var objectsDescriptors = new LinkedList<IObjectDescriptor>();
            var notesDescriptorsNodes = new Dictionary<int, NoteOnsHolder>();

            foreach (var timedObject in timedObjects)
            {
                if (completeObjectsAllowed && !(timedObject is TimedEvent))
                {
                    if (objectsDescriptors.Count == 0)
                        yield return timedObject;
                    else
                        objectsDescriptors.AddLast(new CompleteObjectDescriptor(timedObject));

                    continue;
                }

                var timedEvent = (TimedEvent)timedObject;

                switch (timedEvent.Event.EventType)
                {
                    case MidiEventType.NoteOn:
                        {
                            var noteId = GetNoteEventId((NoteOnEvent)timedEvent.Event);
                            var node = objectsDescriptors.AddLast(new NoteDescriptor(timedEvent));

                            NoteOnsHolder noteOnsHolder;
                            if (!notesDescriptorsNodes.TryGetValue(noteId, out noteOnsHolder))
                                notesDescriptorsNodes.Add(noteId, noteOnsHolder = new NoteOnsHolder(settings.NoteStartDetectionPolicy));

                            noteOnsHolder.Add(node);
                        }
                        break;
                    case MidiEventType.NoteOff:
                        {
                            var noteId = GetNoteEventId((NoteOffEvent)timedEvent.Event);

                            NoteOnsHolder noteOnsHolder;
                            LinkedListNode<IObjectDescriptor> node;

                            if (!notesDescriptorsNodes.TryGetValue(noteId, out noteOnsHolder) || noteOnsHolder.Count == 0 || (node = noteOnsHolder.GetNext()).List == null)
                            {
                                objectsDescriptors.AddLast(new TimedEventDescriptor(timedEvent));
                                break;
                            }

                            ((NoteDescriptor)node.Value).NoteOffTimedEvent = timedEvent;

                            var previousNode = node.Previous;
                            if (previousNode != null)
                                break;

                            for (var n = node; n != null;)
                            {
                                if (!n.Value.IsCompleted)
                                    break;

                                yield return n.Value.GetObject(constructor);

                                var next = n.Next;
                                objectsDescriptors.Remove(n);
                                n = next;
                            }
                        }
                        break;
                    default:
                        {
                            if (objectsDescriptors.Count == 0)
                                yield return timedEvent;
                            else
                                objectsDescriptors.AddLast(new TimedEventDescriptor(timedEvent));
                        }
                        break;
                }
            }

            foreach (var objectDescriptor in objectsDescriptors)
            {
                yield return objectDescriptor.GetObject(constructor);
            }
        }

        private static int GetNoteEventId(NoteEvent noteEvent)
        {
            return noteEvent.Channel * 1000 + noteEvent.NoteNumber;
        }

        #endregion
    }
}
