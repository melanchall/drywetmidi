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

            protected ChordDescriptor(Note firstNote, int notesMinCount)
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
        private readonly NoteDetectionSettings _noteDetectionSettings;
        private readonly bool _useCustomConstructor;

        #endregion

        #region Constructor

        public ChordsBuilder(
            ChordDetectionSettings chordDetectionSettings,
            NoteDetectionSettings noteDetectionSettings)
        {
            _chordDetectionSettings = chordDetectionSettings ?? new ChordDetectionSettings();
            _noteDetectionSettings = noteDetectionSettings ?? new NoteDetectionSettings();
            _useCustomConstructor = _chordDetectionSettings.Constructor != null;
        }

        #endregion

        #region Methods

        public IEnumerable<TimedObjectAt<Chord>> GetChordsLazy(IEnumerable<TimedObjectAt<TimedEvent>> timedEvents, bool collectTimedEvents = false, List<TimedObjectAt<TimedEvent>> collectedTimedEvents = null)
        {
            var chordsDescriptors = new LinkedList<ChordDescriptorIndexed>();
            var chordsDescriptorsByChannel = new LinkedListNode<ChordDescriptorIndexed>[FourBitNumber.MaxValue + 1];

            var notesBuilder = new NotesBuilder(_noteDetectionSettings);
            var notes = notesBuilder.GetNotesLazy(timedEvents, collectTimedEvents, collectedTimedEvents);

            foreach (var noteTuple in notes)
            {
                var note = noteTuple.Object;
                var chordDescriptorNode = chordsDescriptorsByChannel[note.Channel];

                if (chordDescriptorNode == null || chordDescriptorNode.List == null)
                {
                    CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, note, noteTuple.AtIndex);
                }
                else
                {
                    var chordDescriptor = chordDescriptorNode.Value;
                    if (CanNoteBeAddedToChord(chordDescriptor, note, _chordDetectionSettings.NotesTolerance, noteTuple.AtIndex))
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
                                yield return new TimedObjectAt<Chord>(timedObjectX, chordDescriptorNode.Value.EventsCollectionIndex);
                            }
                        }

                        CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, note, noteTuple.AtIndex);
                    }
                }
            }

            foreach (var chord in GetChords(chordsDescriptors.First, chordsDescriptors, false))
            {
                yield return new TimedObjectAt<Chord>(chord, chordsDescriptors.First.Value.EventsCollectionIndex);
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
                    yield return _useCustomConstructor
                        ? _chordDetectionSettings.Constructor(new ChordData(chordDescriptor.Notes))
                        : new Chord(chordDescriptor.Notes);

                var nextChordDescriptorNode = chordDescriptorNode.Next;
                chordsDescriptors.Remove(chordDescriptorNode);
                chordDescriptorNode = nextChordDescriptorNode;
            }
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

        private static bool CanNoteBeAddedToChord(ChordDescriptorIndexed chordDescriptor, Note note, long notesTolerance, int eventsCollectionIndex)
        {
            return note.Time - chordDescriptor.Time <= notesTolerance && chordDescriptor.EventsCollectionIndex == eventsCollectionIndex;
        }

        #endregion
    }
}
