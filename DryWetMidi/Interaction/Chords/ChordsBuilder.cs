using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class ChordsBuilder
    {
        #region Nested types

        private class ChordDescriptor
        {
            private readonly int _notesMinCount;

            public ChordDescriptor(Note firstNote, int notesMinCount)
            {
                Time = firstNote.Time;
                Notes.Add(firstNote);

                _notesMinCount = notesMinCount;
            }

            public long Time { get; }

            public List<Note> Notes { get; } = new List<Note>(3);

            public bool IsSealed { get; set; }

            public bool IsCompleted => Notes.Count >= _notesMinCount;
        }

        private sealed class ChordDescriptorIndexed : ChordDescriptor
        {
            public ChordDescriptorIndexed(Note firstNote, int notesMinCount)
                : base(firstNote, notesMinCount)
            {
            }

            public int EventsCollectionIndex { get; set; }
        }

        #endregion

        #region Fields

        private readonly ChordDetectionSettings _chordDetectionSettings;

        #endregion

        #region Constructor

        public ChordsBuilder(ChordDetectionSettings chordDetectionSettings)
        {
            _chordDetectionSettings = chordDetectionSettings ?? new ChordDetectionSettings();
        }

        #endregion

        #region Methods

        public IEnumerable<Chord> GetChordsLazy(IEnumerable<TimedEvent> timedEvents, bool collectTimedEvents = false, List<TimedEvent> collectedTimedEvents = null)
        {
            var chordsDescriptors = new LinkedList<ChordDescriptor>();
            var chordsDescriptorsByChannel = new LinkedListNode<ChordDescriptor>[FourBitNumber.MaxValue + 1];

            var notesBuilder = new NotesBuilder(_chordDetectionSettings.NoteDetectionSettings);
            var notes = notesBuilder.GetNotesLazy(timedEvents, collectTimedEvents, collectedTimedEvents);

            foreach (var note in notes)
            {
                var chordDescriptorNode = chordsDescriptorsByChannel[note.Channel];

                if (chordDescriptorNode == null || chordDescriptorNode.List == null)
                {
                    CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, note);
                }
                else
                {
                    var chordDescriptor = chordDescriptorNode.Value;
                    if (CanNoteBeAddedToChord(chordDescriptor, note, _chordDetectionSettings.NotesTolerance))
                    {
                        chordDescriptor.Notes.Add(note);
                    }
                    else
                    {
                        chordDescriptor.IsSealed = true;

                        if (chordDescriptorNode.Previous == null)
                        {
                            foreach (var timedObjectX in GetChords(chordDescriptorNode, chordsDescriptors, true))
                            {
                                yield return timedObjectX;
                            }
                        }

                        CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, note);
                    }
                }
            }

            foreach (var chord in GetChords(chordsDescriptors.First, chordsDescriptors, false))
            {
                yield return chord;
            }
        }

        public IEnumerable<Chord> GetChordsLazy(IEnumerable<Tuple<TimedEvent, int>> timedEvents, bool collectTimedEvents = false, List<Tuple<TimedEvent, int>> collectedTimedEvents = null)
        {
            var chordsDescriptors = new LinkedList<ChordDescriptorIndexed>();
            var chordsDescriptorsByChannel = new LinkedListNode<ChordDescriptorIndexed>[FourBitNumber.MaxValue + 1];

            var notesBuilder = new NotesBuilder(_chordDetectionSettings.NoteDetectionSettings);
            var notes = notesBuilder.GetIndexedNotesLazy(timedEvents, collectTimedEvents, collectedTimedEvents);

            var eventsCollectionShouldMatch = _chordDetectionSettings.ChordSearchContext == ChordSearchContext.SingleEventsCollection;

            foreach (var noteTuple in notes)
            {
                var note = noteTuple.Item1;
                var chordDescriptorNode = chordsDescriptorsByChannel[note.Channel];

                if (chordDescriptorNode == null || chordDescriptorNode.List == null)
                {
                    CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, note, noteTuple.Item2);
                }
                else
                {
                    var chordDescriptor = chordDescriptorNode.Value;
                    if (CanNoteBeAddedToChord(chordDescriptor, note, _chordDetectionSettings.NotesTolerance, noteTuple.Item2, eventsCollectionShouldMatch))
                    {
                        chordDescriptor.Notes.Add(note);
                    }
                    else
                    {
                        chordDescriptor.IsSealed = true;

                        if (chordDescriptorNode.Previous == null)
                        {
                            foreach (var timedObjectX in GetChords(chordDescriptorNode, chordsDescriptors, true))
                            {
                                yield return timedObjectX;
                            }
                        }

                        CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, note, noteTuple.Item2);
                    }
                }
            }

            foreach (var chord in GetChords(chordsDescriptors.First, chordsDescriptors, false))
            {
                yield return chord;
            }
        }

        private IEnumerable<Chord> GetChords<TDescriptor>(
            LinkedListNode<TDescriptor> startChordDescriptorNode,
            LinkedList<TDescriptor> chordsDescriptors,
            bool getSealedOnly)
            where TDescriptor : ChordDescriptor
        {
            for (var chordDescriptorNode = startChordDescriptorNode; chordDescriptorNode != null;)
            {
                var chordDescriptor = chordDescriptorNode.Value;
                if (getSealedOnly && !chordDescriptor.IsSealed)
                    break;

                if (chordDescriptor.IsCompleted)
                    yield return new Chord(chordDescriptor.Notes);

                var nextChordDescriptorNode = chordDescriptorNode.Next;
                chordsDescriptors.Remove(chordDescriptorNode);
                chordDescriptorNode = nextChordDescriptorNode;
            }
        }

        private void CreateChordDescriptor(
            LinkedList<ChordDescriptor> chordsDescriptors,
            LinkedListNode<ChordDescriptor>[] chordsDescriptorsByChannel,
            Note note)
        {
            var chordDescriptor = new ChordDescriptor(note, _chordDetectionSettings.NotesMinCount);
            chordsDescriptorsByChannel[note.Channel] = chordsDescriptors.AddLast(chordDescriptor);
        }

        private void CreateChordDescriptor(
            LinkedList<ChordDescriptorIndexed> chordsDescriptors,
            LinkedListNode<ChordDescriptorIndexed>[] chordsDescriptorsByChannel,
            Note note,
            int noteOnIndex)
        {
            var chordDescriptor = new ChordDescriptorIndexed(note, _chordDetectionSettings.NotesMinCount) { EventsCollectionIndex = noteOnIndex };
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
