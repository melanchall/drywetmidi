using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class NotesBuilder
    {
        #region Nested types

        private class NoteDescriptor
        {
            public NoteDescriptor(TimedEvent noteOnTimedEvent)
            {
                NoteOnTimedEvent = noteOnTimedEvent;
            }

            public TimedEvent NoteOnTimedEvent { get; }

            public TimedEvent NoteOffTimedEvent { get; set; }

            public bool IsCompleted => NoteOffTimedEvent != null;

            public Note GetNote(Func<NoteData, Note> constructor)
            {
                return IsCompleted
                    ? (constructor == null
                        ? new Note(NoteOnTimedEvent, NoteOffTimedEvent, false)
                        : constructor(new NoteData(NoteOnTimedEvent, NoteOffTimedEvent)))
                    : null;
            }
        }

        private sealed class IndexedNoteDescriptor : NoteDescriptor
        {
            public IndexedNoteDescriptor(TimedEvent noteOnTimedEvent, int eventsCollectionIndex)
                : base(noteOnTimedEvent)
            {
                EventsCollectionIndex = eventsCollectionIndex;
            }

            public int EventsCollectionIndex { get; }

            public TimedObjectAt<Note> GetIndexedNote(Func<NoteData, Note> constructor)
            {
                return IsCompleted
                    ? new TimedObjectAt<Note>(
                        (constructor == null
                            ? new Note(NoteOnTimedEvent, NoteOffTimedEvent, false)
                            : constructor(new NoteData(NoteOnTimedEvent, NoteOffTimedEvent))),
                        EventsCollectionIndex)
                    : null;
            }
        }

        private abstract class NoteOnsHolderBase<TDescriptor> where TDescriptor : NoteDescriptor
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

        private sealed class NoteOnsHolder : NoteOnsHolderBase<NoteDescriptor>
        {
            public NoteOnsHolder(NoteStartDetectionPolicy noteStartDetectionPolicy)
                : base(noteStartDetectionPolicy)
            {
            }
        }

        private sealed class IndexedNoteOnsHolder : NoteOnsHolderBase<IndexedNoteDescriptor>
        {
            public IndexedNoteOnsHolder(NoteStartDetectionPolicy noteStartDetectionPolicy)
                : base(noteStartDetectionPolicy)
            {
            }
        }

        #endregion

        #region Fields

        private readonly NoteDetectionSettings _noteDetectionSettings;

        #endregion

        #region Constructor

        public NotesBuilder(NoteDetectionSettings noteDetectionSettings)
        {
            _noteDetectionSettings = noteDetectionSettings ?? new NoteDetectionSettings();
        }

        #endregion

        #region Methods

        public IEnumerable<Note> GetNotesLazy(IEnumerable<TimedEvent> timedEvents, bool collectTimedEvents = false, List<TimedEvent> collectedTimedEvents = null)
        {
            var notesDescriptors = new LinkedList<NoteDescriptor>();
            var notesDescriptorsNodes = new Dictionary<int, NoteOnsHolder>();

            foreach (var timedEvent in timedEvents)
            {
                if (collectTimedEvents)
                    collectedTimedEvents.Add(timedEvent);

                switch (timedEvent.Event.EventType)
                {
                    case MidiEventType.NoteOn:
                        {
                            var noteId = GetNoteEventId((NoteOnEvent)timedEvent.Event);
                            var node = notesDescriptors.AddLast(new NoteDescriptor(timedEvent));

                            NoteOnsHolder noteOnsHolder;
                            if (!notesDescriptorsNodes.TryGetValue(noteId, out noteOnsHolder))
                                notesDescriptorsNodes.Add(noteId, noteOnsHolder = new NoteOnsHolder(_noteDetectionSettings.NoteStartDetectionPolicy));

                            noteOnsHolder.Add(node);
                        }
                        break;

                    case MidiEventType.NoteOff:
                        {
                            var noteId = GetNoteEventId((NoteOffEvent)timedEvent.Event);

                            NoteOnsHolder noteOnsHolder;
                            LinkedListNode<NoteDescriptor> node;

                            if (!notesDescriptorsNodes.TryGetValue(noteId, out noteOnsHolder) || noteOnsHolder.Count == 0 || (node = noteOnsHolder.GetNext()).List == null)
                                continue;

                            node.Value.NoteOffTimedEvent = timedEvent;

                            var previousNode = node.Previous;
                            if (previousNode != null)
                                continue;

                            for (var n = node; n != null;)
                            {
                                if (!n.Value.IsCompleted)
                                    break;

                                yield return n.Value.GetNote(_noteDetectionSettings.Constructor);

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
                var note = noteDescriptor.GetNote(_noteDetectionSettings.Constructor);
                if (note != null)
                    yield return note;
            }
        }

        public IEnumerable<TimedObjectAt<Note>> GetNotesLazy(
            IEnumerable<TimedObjectAt<TimedEvent>> timedEvents,
            bool collectTimedEvents = false,
            List<TimedObjectAt<TimedEvent>> collectedTimedEvents = null)
        {
            var notesDescriptors = new LinkedList<IndexedNoteDescriptor>();
            var notesDescriptorsNodes = new Dictionary<Tuple<int, int>, IndexedNoteOnsHolder>();

            foreach (var timedEventTuple in timedEvents)
            {
                if (collectTimedEvents)
                    collectedTimedEvents.Add(timedEventTuple);

                var timedEvent = timedEventTuple.Object;
                switch (timedEvent.Event.EventType)
                {
                    case MidiEventType.NoteOn:
                        {
                            var noteId = GetNoteEventId((NoteOnEvent)timedEvent.Event);
                            var noteFullId = Tuple.Create(noteId, timedEventTuple.AtIndex);
                            var node = notesDescriptors.AddLast(new IndexedNoteDescriptor(timedEvent, timedEventTuple.AtIndex));

                            IndexedNoteOnsHolder noteOnsHolder;
                            if (!notesDescriptorsNodes.TryGetValue(noteFullId, out noteOnsHolder))
                                notesDescriptorsNodes.Add(noteFullId, noteOnsHolder = new IndexedNoteOnsHolder(_noteDetectionSettings.NoteStartDetectionPolicy));

                            noteOnsHolder.Add(node);
                        }
                        break;
                    case MidiEventType.NoteOff:
                        {
                            var noteId = GetNoteEventId((NoteOffEvent)timedEvent.Event);
                            var noteFullId = Tuple.Create(noteId, timedEventTuple.AtIndex);

                            IndexedNoteOnsHolder noteOnsHolder;
                            LinkedListNode<IndexedNoteDescriptor> node;

                            if (!notesDescriptorsNodes.TryGetValue(noteFullId, out noteOnsHolder) || noteOnsHolder.Count == 0 || (node = noteOnsHolder.GetNext()).List == null)
                                continue;

                            node.Value.NoteOffTimedEvent = timedEvent;

                            var previousNode = node.Previous;
                            if (previousNode != null)
                                continue;

                            for (var n = node; n != null;)
                            {
                                if (!n.Value.IsCompleted)
                                    break;

                                yield return n.Value.GetIndexedNote(_noteDetectionSettings.Constructor);

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
                var note = noteDescriptor.GetIndexedNote(_noteDetectionSettings.Constructor);
                if (note != null)
                    yield return note;
            }
        }

        private static int GetNoteEventId(NoteEvent noteEvent)
        {
            return noteEvent.Channel * 1000 + noteEvent.NoteNumber;
        }

        #endregion
    }
}
