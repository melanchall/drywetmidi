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

            public NoteOnsHolderBase(NoteStartDetectionPolicy noteStartDetectionPolicy)
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

            ITimedObject GetObject();
        }

        private interface IObjectDescriptorIndexed : IObjectDescriptor
        {
            Tuple<ITimedObject, int, int> GetIndexedObject();
        }

        private class NoteDescriptor : IObjectDescriptor
        {
            public NoteDescriptor(TimedEvent noteOnTimedEvent)
            {
                NoteOnTimedEvent = noteOnTimedEvent;
            }

            public TimedEvent NoteOnTimedEvent { get; }

            public TimedEvent NoteOffTimedEvent { get; set; }

            public bool IsCompleted => NoteOffTimedEvent != null;

            public ITimedObject GetObject()
            {
                return IsCompleted ? new Note(NoteOnTimedEvent, NoteOffTimedEvent) : (ITimedObject)NoteOnTimedEvent;
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

            public ITimedObject GetObject()
            {
                return _timedObject;
            }
        }

        private sealed class NoteDescriptorIndexed : NoteDescriptor, IObjectDescriptorIndexed
        {
            private readonly int _noteOnIndex;

            public NoteDescriptorIndexed(TimedEvent noteOnTimedEvent, int noteOnIndex)
                : base(noteOnTimedEvent)
            {
                _noteOnIndex = noteOnIndex;
                NoteOffIndex = _noteOnIndex;
            }

            public int NoteOffIndex { get; set; }

            public Tuple<ITimedObject, int, int> GetIndexedObject()
            {
                return Tuple.Create(GetObject(), _noteOnIndex, NoteOffIndex);
            }
        }

        private class TimedEventDescriptor : IObjectDescriptor
        {
            public TimedEventDescriptor(TimedEvent timedEvent)
            {
                TimedEvent = timedEvent;
            }

            public TimedEvent TimedEvent { get; }

            public bool IsCompleted { get; } = true;

            public ITimedObject GetObject()
            {
                return TimedEvent;
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

            public Tuple<ITimedObject, int, int> GetIndexedObject()
            {
                return Tuple.Create(GetObject(), _index, _index);
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
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <returns>An instance of the <see cref="NotesManager"/> that can be used to manage
        /// notes represented by the <paramref name="eventsCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static NotesManager ManageNotes(this EventsCollection eventsCollection, NoteDetectionSettings settings = null, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return new NotesManager(eventsCollection, settings, sameTimeEventsComparison);
        }

        /// <summary>
        /// Creates an instance of the <see cref="NotesManager"/> initializing it with the
        /// events collection of the specified track chunk and comparison delegate for events
        /// that have same time.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> that holds notes to manage.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <returns>An instance of the <see cref="NotesManager"/> that can be used to manage
        /// notes represented by the <paramref name="trackChunk"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static NotesManager ManageNotes(this TrackChunk trackChunk, NoteDetectionSettings settings = null, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.ManageNotes(settings, sameTimeEventsComparison);
        }

        /// <summary>
        /// Gets notes contained in the specified collection of <see cref="MidiEvent"/>.
        /// </summary>
        /// <param name="midiEvents">Collection of <see cref="MidiEvent"/> to search for notes.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Collection of notes contained in <paramref name="midiEvents"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is <c>null</c>.</exception>
        public static ICollection<Note> GetNotes(this IEnumerable<MidiEvent> midiEvents, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            var result = new List<Note>();
            var notesBuilder = new NotesBuilder(settings);

            var notes = notesBuilder.GetNotesLazy(midiEvents.GetTimedEventsLazy());

            result.AddRange(notes);
            return result;
        }

        /// <summary>
        /// Gets notes contained in the specified <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Collection of notes contained in <paramref name="eventsCollection"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static ICollection<Note> GetNotes(this EventsCollection eventsCollection, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            var result = new List<Note>();
            var notesBuilder = new NotesBuilder(settings);

            var notes = notesBuilder.GetNotesLazy(eventsCollection.GetTimedEventsLazy());
            
            result.AddRange(notes);
            return result;
        }

        /// <summary>
        /// Gets notes contained in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Collection of notes contained in <paramref name="trackChunk"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static ICollection<Note> GetNotes(this TrackChunk trackChunk, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.GetNotes(settings);
        }

        /// <summary>
        /// Gets notes contained in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Track chunks to search for notes.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Collection of notes contained in <paramref name="trackChunks"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static ICollection<Note> GetNotes(this IEnumerable<TrackChunk> trackChunks, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var eventsCollections = trackChunks.Select(c => c.Events).ToArray();

            switch (eventsCollections.Length)
            {
                case 0: return new Note[0];
                case 1: return eventsCollections[0].GetNotes(settings);
            }

            var eventsCount = eventsCollections.Sum(e => e.Count);
            var result = new List<Note>(eventsCount / 3);
            var notesBuilder = new NotesBuilder(settings);

            var notes = notesBuilder.GetNotesLazy(eventsCollections.GetTimedEventsLazy(eventsCount));

            result.AddRange(notes);
            return result;
        }

        /// <summary>
        /// Gets notes contained in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Collection of notes contained in <paramref name="file"/> ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        public static ICollection<Note> GetNotes(this MidiFile file, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().GetNotes(settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(this EventsCollection eventsCollection, Action<Note> action, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);

            return eventsCollection.ProcessNotes(action, note => true, settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(this EventsCollection eventsCollection, Action<Note> action, Predicate<Note> match, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return eventsCollection.ProcessNotes(action, match, settings, true);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(this TrackChunk trackChunk, Action<Note> action, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunk.ProcessNotes(action, note => true, settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(this TrackChunk trackChunk, Action<Note> action, Predicate<Note> match, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunk.Events.ProcessNotes(action, match, settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the collection of
        /// <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(this IEnumerable<TrackChunk> trackChunks, Action<Note> action, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunks.ProcessNotes(action, note => true, settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the collection of
        /// <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(this IEnumerable<TrackChunk> trackChunks, Action<Note> action, Predicate<Note> match, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunks.ProcessNotes(action, match, settings, true);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(this MidiFile file, Action<Note> action, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);

            return file.ProcessNotes(action, note => true, settings);
        }

        /// <summary>
        /// Performs the specified action on each <see cref="Note"/> contained in the <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes to process.</param>
        /// <param name="action">The action to perform on each <see cref="Note"/> contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to process.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of processed notes.</returns>
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
        public static int ProcessNotes(this MidiFile file, Action<Note> action, Predicate<Note> match, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().ProcessNotes(action, match, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes to remove.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of removed notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static int RemoveNotes(this EventsCollection eventsCollection, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return eventsCollection.RemoveNotes(note => true, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of removed notes.</returns>
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
        public static int RemoveNotes(this EventsCollection eventsCollection, Predicate<Note> match, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(match), match);

            var notesToRemoveCount = eventsCollection.ProcessNotes(
                n => n.TimedNoteOnEvent.Event.Flag = n.TimedNoteOffEvent.Event.Flag = true,
                match,
                settings,
                false);

            if (notesToRemoveCount == 0)
                return 0;

            eventsCollection.RemoveTimedEvents(e => e.Event.Flag);
            return notesToRemoveCount;
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes to remove.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of removed notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static int RemoveNotes(this TrackChunk trackChunk, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.RemoveNotes(note => true, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of removed notes.</returns>
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
        public static int RemoveNotes(this TrackChunk trackChunk, Predicate<Note> match, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunk.Events.RemoveNotes(match, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for notes to remove.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of removed notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static int RemoveNotes(this IEnumerable<TrackChunk> trackChunks, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return trackChunks.RemoveNotes(note => true, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of removed notes.</returns>
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
        public static int RemoveNotes(this IEnumerable<TrackChunk> trackChunks, Predicate<Note> match, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(match), match);

            var notesToRemoveCount = trackChunks.ProcessNotes(
                n => n.TimedNoteOnEvent.Event.Flag = n.TimedNoteOffEvent.Event.Flag = true,
                match,
                settings,
                false);

            if (notesToRemoveCount == 0)
                return 0;

            trackChunks.RemoveTimedEvents(e => e.Event.Flag);
            return notesToRemoveCount;
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes to remove.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of removed notes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        public static int RemoveNotes(this MidiFile file, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.RemoveNotes(note => true, settings);
        }

        /// <summary>
        /// Removes all the <see cref="Note"/> that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for notes to remove.</param>
        /// <param name="match">The predicate that defines the conditions of the <see cref="Note"/> to remove.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Count of removed notes.</returns>
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
        public static int RemoveNotes(this MidiFile file, Predicate<Note> match, NoteDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().RemoveNotes(match, settings);
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

        internal static int ProcessNotes(
            this IEnumerable<TrackChunk> trackChunks,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings noteDetectionSettings,
            bool canTimeOrLengthBeChanged)
        {
            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            var iMatched = 0;

            var timeOrLengthChanged = false;
            var collectedTimedEvents = canTimeOrLengthBeChanged ? new List<Tuple<TimedEvent, int>>(eventsCount) : null;

            var notesBuilder = new NotesBuilder(noteDetectionSettings);
            var notes = notesBuilder.GetNotesLazy(eventsCollections.GetTimedEventsLazy(eventsCount, false), canTimeOrLengthBeChanged, collectedTimedEvents);

            foreach (var note in notes)
            {
                if (match(note))
                {
                    var startTime = note.TimedNoteOnEvent.Time;
                    var endTime = note.TimedNoteOffEvent.Time;

                    action(note);

                    timeOrLengthChanged |= note.TimedNoteOnEvent.Time != startTime || note.TimedNoteOffEvent.Time != endTime;

                    iMatched++;
                }
            }

            if (timeOrLengthChanged)
                eventsCollections.SortAndUpdateEvents(collectedTimedEvents);

            return iMatched;
        }

        internal static int ProcessNotes(
            this EventsCollection eventsCollection,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings noteDetectionSettings,
            bool canTimeOrLengthBeChanged)
        {
            var iMatched = 0;

            var timeOrLengthChanged = false;
            var collectedTimedEvents = canTimeOrLengthBeChanged ? new List<TimedEvent>(eventsCollection.Count) : null;

            var notesBuilder = new NotesBuilder(noteDetectionSettings);
            var notes = notesBuilder.GetNotesLazy(eventsCollection.GetTimedEventsLazy(false), canTimeOrLengthBeChanged, collectedTimedEvents);

            foreach (var note in notes)
            {
                if (match(note))
                {
                    var startTime = note.TimedNoteOnEvent.Time;
                    var endTime = note.TimedNoteOffEvent.Time;

                    action(note);

                    timeOrLengthChanged |= note.TimedNoteOnEvent.Time != startTime || note.TimedNoteOffEvent.Time != endTime;

                    iMatched++;
                }
            }

            if (timeOrLengthChanged)
                eventsCollection.SortAndUpdateEvents(collectedTimedEvents);

            return iMatched;
        }

        internal static IEnumerable<Tuple<ITimedObject, int, int>> GetNotesAndTimedEventsLazy(
            this IEnumerable<Tuple<TimedEvent, int>> timedEvents,
            NoteDetectionSettings settings)
        {
            var objectsDescriptors = new LinkedList<IObjectDescriptorIndexed>();
            var notesDescriptorsNodes = new Dictionary<Tuple<int, int>, NoteOnsHolderIndexed>();

            var respectEventsCollectionIndex = settings.NoteSearchContext == NoteSearchContext.SingleEventsCollection;

            foreach (var timedEventTuple in timedEvents)
            {
                var timedEvent = timedEventTuple.Item1;
                switch (timedEvent.Event.EventType)
                {
                    case MidiEventType.NoteOn:
                        {
                            var noteId = GetNoteEventId((NoteOnEvent)timedEvent.Event);
                            var noteFullId = Tuple.Create(noteId, respectEventsCollectionIndex ? timedEventTuple.Item2 : -1);
                            var node = objectsDescriptors.AddLast(new NoteDescriptorIndexed(timedEvent, timedEventTuple.Item2));

                            NoteOnsHolderIndexed noteOnsHolder;
                            if (!notesDescriptorsNodes.TryGetValue(noteFullId, out noteOnsHolder))
                                notesDescriptorsNodes.Add(noteFullId, noteOnsHolder = new NoteOnsHolderIndexed(settings.NoteStartDetectionPolicy));

                            noteOnsHolder.Add(node);
                        }
                        break;
                    case MidiEventType.NoteOff:
                        {
                            var noteId = GetNoteEventId((NoteOffEvent)timedEvent.Event);
                            var noteFullId = Tuple.Create(noteId, respectEventsCollectionIndex ? timedEventTuple.Item2 : -1);

                            NoteOnsHolderIndexed noteOnsHolder;
                            LinkedListNode<IObjectDescriptorIndexed> node;

                            if (!notesDescriptorsNodes.TryGetValue(noteFullId, out noteOnsHolder) || noteOnsHolder.Count == 0 || (node = noteOnsHolder.GetNext()).List == null)
                            {
                                objectsDescriptors.AddLast(new TimedEventDescriptorIndexed(timedEvent, timedEventTuple.Item2));
                                break;
                            }

                            var noteDescriptorIndexed = (NoteDescriptorIndexed)node.Value;
                            noteDescriptorIndexed.NoteOffTimedEvent = timedEvent;
                            noteDescriptorIndexed.NoteOffIndex = timedEventTuple.Item2;

                            var previousNode = node.Previous;
                            if (previousNode != null)
                                break;

                            for (var n = node; n != null;)
                            {
                                if (!n.Value.IsCompleted)
                                    break;

                                yield return n.Value.GetIndexedObject();

                                var next = n.Next;
                                objectsDescriptors.Remove(n);
                                n = next;
                            }
                        }
                        break;
                    default:
                        {
                            if (objectsDescriptors.Count == 0)
                                yield return Tuple.Create((ITimedObject)timedEvent, timedEventTuple.Item2, timedEventTuple.Item2);
                            else
                                objectsDescriptors.AddLast(new TimedEventDescriptorIndexed(timedEvent, timedEventTuple.Item2));
                        }
                        break;
                }
            }

            foreach (var objectDescriptor in objectsDescriptors)
            {
                yield return objectDescriptor.GetIndexedObject();
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

                            for (var n = node; n != null; )
                            {
                                if (!n.Value.IsCompleted)
                                    break;

                                yield return n.Value.GetObject();

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
                yield return objectDescriptor.GetObject();
            }
        }

        private static int GetNoteEventId(NoteEvent noteEvent)
        {
            return noteEvent.Channel * 1000 + noteEvent.NoteNumber;
        }

        #endregion
    }
}
