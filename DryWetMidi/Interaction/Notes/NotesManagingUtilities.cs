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

        private abstract class NoteOnIndexesHolder
        {
            private const int DefaultCapacity = 2;

            public abstract int Count { get; }

            public abstract void Add(int noteOnIndex);

            public abstract int GetNext();

            public static NoteOnIndexesHolder Create(NoteStartDetectionPolicy policy, int firstIndex, int secondIndex)
            {
                NoteOnIndexesHolder holder = policy == NoteStartDetectionPolicy.LastNoteOn
                    ? (NoteOnIndexesHolder)new NoteOnIndexesHolderStack(DefaultCapacity)
                    : new NoteOnIndexesHolderQueue(DefaultCapacity);

                holder.Add(firstIndex);
                holder.Add(secondIndex);
                return holder;
            }
        }

        private sealed class NoteOnIndexesHolderStack : NoteOnIndexesHolder
        {
            private readonly Stack<int> _indexes;

            public NoteOnIndexesHolderStack(int capacity)
            {
                _indexes = new Stack<int>(capacity);
            }

            public override int Count => _indexes.Count;

            public override void Add(int noteOnIndex) => _indexes.Push(noteOnIndex);

            public override int GetNext() => _indexes.Pop();
        }

        private sealed class NoteOnIndexesHolderQueue : NoteOnIndexesHolder
        {
            private readonly Queue<int> _indexes;

            public NoteOnIndexesHolderQueue(int capacity)
            {
                _indexes = new Queue<int>(capacity);
            }

            public override int Count => _indexes.Count;

            public override void Add(int noteOnIndex) => _indexes.Enqueue(noteOnIndex);

            public override int GetNext() => _indexes.Dequeue();
        }

        private struct NoteOnEntry
        {
            public int SingleIndex;
            public NoteOnIndexesHolder Holder;
        }

        private readonly struct NoteOnRecord
        {
            public readonly TimedEvent TimedEvent;
            public readonly int Seq;

            public NoteOnRecord(TimedEvent timedEvent, int seq)
            {
                TimedEvent = timedEvent;
                Seq = seq;
            }
        }

        private abstract class NoteOnTimedEventsHolder
        {
            private const int DefaultCapacity = 2;

            public abstract int Count { get; }

            public abstract void Add(NoteOnRecord record);

            public abstract NoteOnRecord GetNext();

            public static NoteOnTimedEventsHolder Create(NoteStartDetectionPolicy policy, NoteOnRecord first, NoteOnRecord second)
            {
                NoteOnTimedEventsHolder holder = policy == NoteStartDetectionPolicy.LastNoteOn
                    ? (NoteOnTimedEventsHolder)new NoteOnTimedEventsHolderStack(DefaultCapacity)
                    : new NoteOnTimedEventsHolderQueue(DefaultCapacity);

                holder.Add(first);
                holder.Add(second);
                return holder;
            }
        }

        private sealed class NoteOnTimedEventsHolderStack : NoteOnTimedEventsHolder
        {
            private readonly Stack<NoteOnRecord> _records;

            public NoteOnTimedEventsHolderStack(int capacity)
            {
                _records = new Stack<NoteOnRecord>(capacity);
            }

            public override int Count => _records.Count;

            public override void Add(NoteOnRecord record) => _records.Push(record);

            public override NoteOnRecord GetNext() => _records.Pop();
        }

        private sealed class NoteOnTimedEventsHolderQueue : NoteOnTimedEventsHolder
        {
            private readonly Queue<NoteOnRecord> _records;

            public NoteOnTimedEventsHolderQueue(int capacity)
            {
                _records = new Queue<NoteOnRecord>(capacity);
            }

            public override int Count => _records.Count;

            public override void Add(NoteOnRecord record) => _records.Enqueue(record);

            public override NoteOnRecord GetNext() => _records.Dequeue();
        }

        private struct NoteOnTimedEventsEntry
        {
            public NoteOnRecord Single;
            public NoteOnTimedEventsHolder Holder;
        }

        private enum PendingObjectDescriptorType : byte
        {
            Note = 0,
            TimedEvent,
            CompleteObject
        }

        private struct PendingObjectDescriptor
        {
            public PendingObjectDescriptorType Type;
            public TimedEvent TimedEvent;
            public TimedEvent NoteOffTimedEvent { get; set; }
            public ITimedObject TimedObject;

            public bool IsCompleted => Type != PendingObjectDescriptorType.Note || NoteOffTimedEvent != null;

            public ITimedObject GetObject(Func<NoteData, Note> constructor)
            {
                switch (Type)
                {
                    case PendingObjectDescriptorType.Note:
                        if (NoteOffTimedEvent == null)
                            return TimedEvent;

                        var note = constructor != null
                            ? constructor(new NoteData(TimedEvent, NoteOffTimedEvent))
                            : null;

                        if (note == null)
                            note = new Note(TimedEvent, NoteOffTimedEvent, false);

                        return note;
                    case PendingObjectDescriptorType.CompleteObject:
                        return TimedObject;
                    default:
                        return TimedEvent;
                }
            }

            public static PendingObjectDescriptor CreateNote(TimedEvent timedEvent)
            {
                return new PendingObjectDescriptor
                {
                    Type = PendingObjectDescriptorType.Note,
                    TimedEvent = timedEvent
                };
            }

            public static PendingObjectDescriptor CreateTimedEvent(TimedEvent timedEvent)
            {
                return new PendingObjectDescriptor
                {
                    Type = PendingObjectDescriptorType.TimedEvent,
                    TimedEvent = timedEvent
                };
            }

            public static PendingObjectDescriptor CreateCompleteObject(ITimedObject timedObject)
            {
                return new PendingObjectDescriptor
                {
                    Type = PendingObjectDescriptorType.CompleteObject,
                    TimedObject = timedObject
                };
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

            return GetNotesOnly(
                midiEvents.GetTimedEventsLazy(timedEventDetectionSettings, 0),
                settings ?? new NoteDetectionSettings());
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

            var notes = trackChunks
                .Select((c, i) => GetNotesAndTimedEventsLazy(c.Events.GetTimedEventsLazy(timedEventDetectionSettings, i), settings).OfType<Note>())
                .MergeSortedObjectsCollections();

            return new SortedImmutableCollection<Note>(notes.ToArray());
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
                n => n.TimedNoteOnEvent.Event.MustBeRemoved = n.TimedNoteOffEvent.Event.MustBeRemoved = true,
                match,
                settings,
                timedEventDetectionSettings,
                NoteProcessingHint.None);

            if (notesToRemoveCount == 0)
                return 0;

            eventsCollection.RemoveTimedEvents(e => e.Event.MustBeRemoved);
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
                n => n.TimedNoteOnEvent.Event.MustBeRemoved = n.TimedNoteOffEvent.Event.MustBeRemoved = true,
                match,
                settings,
                timedEventDetectionSettings,
                NoteProcessingHint.None);

            if (notesToRemoveCount == 0)
                return 0;

            trackChunks.RemoveTimedEvents(e => e.Event.MustBeRemoved);
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
            this IEnumerable<EventsCollection> eventsCollections,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings noteDetectionSettings,
            TimedEventDetectionSettings timedEventDetectionSettings,
            NoteProcessingHint hint)
        {
            var processedCount = 0;
            var eventsCollectionIndex = 0;

            var timeOrLengthCanBeChanged = hint.HasFlag(NoteProcessingHint.TimeOrLengthCanBeChanged);

            foreach (var eventsCollection in eventsCollections)
            {
                var matchedCount = 0;

                var timeOrLengthChanged = false;

                var timedEvents = eventsCollection.GetTimedEventsLazy(timedEventDetectionSettings, eventsCollectionIndex, false).ToArray();
                var notes = GetNotesAndTimedEventsLazy(timedEvents, noteDetectionSettings).OfType<Note>();

                foreach (var note in notes)
                {
                    if (!match(note))
                        continue;

                    var startTime = note.TimedNoteOnEvent.Time;
                    var endTime = note.TimedNoteOffEvent.Time;

                    action(note);

                    timeOrLengthChanged |= note.TimedNoteOnEvent.Time != startTime || note.TimedNoteOffEvent.Time != endTime;

                    matchedCount++;
                }

                if (timeOrLengthCanBeChanged && timeOrLengthChanged)
                    eventsCollection.SortAndUpdateEvents(timedEvents);

                processedCount += matchedCount;
                eventsCollectionIndex++;
            }

            return processedCount;
        }

        internal static IEnumerable<ITimedObject> GetNotesAndTimedEventsLazy(
            this IEnumerable<TimedEvent> timedEvents,
            NoteDetectionSettings settings)
        {
            return GetNotesAndTimedEventsLazy(timedEvents, settings, false);
        }

        private static SortedImmutableCollection<Note> GetNotesOnly(
            IEnumerable<TimedEvent> timedEvents,
            NoteDetectionSettings settings)
        {
            var constructor = settings.Constructor;
            var policy = settings.NoteStartDetectionPolicy;

            var noteOns = new Dictionary<int, NoteOnTimedEventsEntry>();
            var result = new List<(Note Note, int Seq)>();
            var isSorted = true;
            var prevTime = long.MinValue;
            var seqCounter = 0;

            foreach (var timedEvent in timedEvents)
            {
                var eventType = timedEvent.Event.EventType;

                if (eventType == MidiEventType.NoteOn)
                {
                    var noteId = ((NoteOnEvent)timedEvent.Event).GetNoteId();
                    var record = new NoteOnRecord(timedEvent, seqCounter++);

                    if (!noteOns.TryGetValue(noteId, out var entry))
                    {
                        noteOns[noteId] = new NoteOnTimedEventsEntry { Single = record };
                    }
                    else if (entry.Holder == null)
                    {
                        noteOns[noteId] = new NoteOnTimedEventsEntry
                        {
                            Holder = NoteOnTimedEventsHolder.Create(policy, entry.Single, record)
                        };
                    }
                    else
                    {
                        entry.Holder.Add(record);
                    }
                }
                else if (eventType == MidiEventType.NoteOff)
                {
                    var noteId = ((NoteOffEvent)timedEvent.Event).GetNoteId();

                    if (!noteOns.TryGetValue(noteId, out var entry))
                        continue;

                    NoteOnRecord noteOnRecord;

                    if (entry.Holder == null)
                    {
                        noteOnRecord = entry.Single;
                        noteOns.Remove(noteId);
                    }
                    else
                    {
                        var holder = entry.Holder;
                        if (holder.Count == 0)
                        {
                            noteOns.Remove(noteId);
                            continue;
                        }

                        noteOnRecord = holder.GetNext();
                        if (holder.Count == 0)
                            noteOns.Remove(noteId);
                    }

                    var note = constructor != null
                        ? constructor(new NoteData(noteOnRecord.TimedEvent, timedEvent)) ?? new Note(noteOnRecord.TimedEvent, timedEvent, false)
                        : new Note(noteOnRecord.TimedEvent, timedEvent, false);

                    if (isSorted)
                    {
                        var time = note.Time;
                        if (time < prevTime)
                            isSorted = false;
                        else
                            prevTime = time;
                    }

                    result.Add((note, noteOnRecord.Seq));
                }
                // Non-note events: not needed for GetNotes, skip to avoid unnecessary work
            }

            // Orphaned NoteOns (no matching NoteOff) are silently dropped, matching the
            // behaviour of the GetNotesAndTimedEventsLazy path where they emit as TimedEvents
            // which are then filtered out by .OfType<Note>().

            if (!isSorted)
                result.Sort((a, b) =>
                {
                    var timeCmp = a.Note.Time.CompareTo(b.Note.Time);
                    return timeCmp != 0 ? timeCmp : a.Seq.CompareTo(b.Seq);
                });

            var notes = new Note[result.Count];
            for (var i = 0; i < result.Count; i++)
                notes[i] = result[i].Note;

            return new SortedImmutableCollection<Note>(notes);
        }

        internal static IEnumerable<ITimedObject> GetNotesAndTimedEventsLazy(
            this IEnumerable<ITimedObject> timedObjects,
            NoteDetectionSettings settings,
            bool completeObjectsAllowed)
        {
            return new SortedLazyCollection<ITimedObject>(GetSortedNotesAndTimedEventsLazy(
                timedObjects,
                settings,
                completeObjectsAllowed));
        }

        private static IEnumerable<ITimedObject> GetSortedNotesAndTimedEventsLazy(
            this IEnumerable<ITimedObject> timedObjects,
            NoteDetectionSettings settings,
            bool completeObjectsAllowed)
        {
            settings = settings ?? new NoteDetectionSettings();
            var constructor = settings.Constructor;

            var objectsDescriptors = new List<PendingObjectDescriptor>();
            var notesDescriptorsNodes = new Dictionary<int, NoteOnEntry>();
            var firstDescriptorIndex = 0;
            var headDescriptorIndex = 0;
            var nextDescriptorIndex = 0;

            void CompactObjectsDescriptors()
            {
                var removeCount = headDescriptorIndex - firstDescriptorIndex;
                if (removeCount == 0)
                    return;

                if (removeCount == objectsDescriptors.Count)
                {
                    objectsDescriptors.Clear();
                    firstDescriptorIndex = headDescriptorIndex;
                    return;
                }

                if (removeCount < 128 && removeCount * 2 < objectsDescriptors.Count)
                    return;

                objectsDescriptors.RemoveRange(0, removeCount);
                firstDescriptorIndex = headDescriptorIndex;
            }

            foreach (var timedObject in timedObjects)
            {
                if (completeObjectsAllowed && !(timedObject is TimedEvent))
                {
                    if (headDescriptorIndex == nextDescriptorIndex)
                        yield return timedObject;
                    else
                    {
                        objectsDescriptors.Add(PendingObjectDescriptor.CreateCompleteObject(timedObject));
                        nextDescriptorIndex++;
                    }

                    continue;
                }

                var timedEvent = (TimedEvent)timedObject;

                switch (timedEvent.Event.EventType)
                {
                    case MidiEventType.NoteOn:
                        {
                            var noteId = ((NoteOnEvent)timedEvent.Event).GetNoteId();
                            var noteDescriptorIndex = nextDescriptorIndex;
                            objectsDescriptors.Add(PendingObjectDescriptor.CreateNote(timedEvent));
                            nextDescriptorIndex++;

                            if (!notesDescriptorsNodes.TryGetValue(noteId, out var entry))
                            {
                                notesDescriptorsNodes[noteId] = new NoteOnEntry { SingleIndex = noteDescriptorIndex };
                            }
                            else if (entry.Holder == null)
                            {
                                notesDescriptorsNodes[noteId] = new NoteOnEntry
                                {
                                    Holder = NoteOnIndexesHolder.Create(settings.NoteStartDetectionPolicy, entry.SingleIndex, noteDescriptorIndex)
                                };
                            }
                            else
                            {
                                entry.Holder.Add(noteDescriptorIndex);
                            }
                        }
                        break;
                    case MidiEventType.NoteOff:
                        {
                            var noteId = ((NoteOffEvent)timedEvent.Event).GetNoteId();

                            if (!notesDescriptorsNodes.TryGetValue(noteId, out var entry))
                            {
                                if (headDescriptorIndex == nextDescriptorIndex)
                                    yield return timedEvent;
                                else
                                {
                                    objectsDescriptors.Add(PendingObjectDescriptor.CreateTimedEvent(timedEvent));
                                    nextDescriptorIndex++;
                                }
                                break;
                            }

                            int noteDescriptorIndex;

                            if (entry.Holder == null)
                            {
                                noteDescriptorIndex = entry.SingleIndex;
                                if (noteDescriptorIndex < headDescriptorIndex)
                                {
                                    notesDescriptorsNodes.Remove(noteId);
                                    if (headDescriptorIndex == nextDescriptorIndex)
                                        yield return timedEvent;
                                    else
                                    {
                                        objectsDescriptors.Add(PendingObjectDescriptor.CreateTimedEvent(timedEvent));
                                        nextDescriptorIndex++;
                                    }
                                    break;
                                }
                                notesDescriptorsNodes.Remove(noteId);
                            }
                            else
                            {
                                var holder = entry.Holder;
                                noteDescriptorIndex = -1;

                                while (holder.Count > 0)
                                {
                                    var candidateDescriptorIndex = holder.GetNext();
                                    if (candidateDescriptorIndex >= headDescriptorIndex)
                                    {
                                        noteDescriptorIndex = candidateDescriptorIndex;
                                        break;
                                    }
                                }

                                if (holder.Count == 0)
                                    notesDescriptorsNodes.Remove(noteId);

                                if (noteDescriptorIndex < 0)
                                {
                                    if (headDescriptorIndex == nextDescriptorIndex)
                                        yield return timedEvent;
                                    else
                                    {
                                        objectsDescriptors.Add(PendingObjectDescriptor.CreateTimedEvent(timedEvent));
                                        nextDescriptorIndex++;
                                    }
                                    break;
                                }
                            }

                            var descriptorIndex = noteDescriptorIndex - firstDescriptorIndex;
                            var noteDescriptor = objectsDescriptors[descriptorIndex];
                            noteDescriptor.NoteOffTimedEvent = timedEvent;
                            objectsDescriptors[descriptorIndex] = noteDescriptor;

                            if (noteDescriptorIndex != headDescriptorIndex)
                                break;

                            for (; headDescriptorIndex < nextDescriptorIndex; headDescriptorIndex++)
                            {
                                var completedDescriptor = objectsDescriptors[headDescriptorIndex - firstDescriptorIndex];
                                if (!completedDescriptor.IsCompleted)
                                    break;

                                yield return completedDescriptor.GetObject(constructor);
                            }

                            CompactObjectsDescriptors();
                        }
                        break;
                    default:
                        {
                            if (headDescriptorIndex == nextDescriptorIndex)
                                yield return timedEvent;
                            else
                            {
                                objectsDescriptors.Add(PendingObjectDescriptor.CreateTimedEvent(timedEvent));
                                nextDescriptorIndex++;
                            }
                        }
                        break;
                }
            }

            for (; headDescriptorIndex < nextDescriptorIndex; headDescriptorIndex++)
            {
                yield return objectsDescriptors[headDescriptorIndex - firstDescriptorIndex].GetObject(constructor);
            }
        }

        #endregion
    }
}
