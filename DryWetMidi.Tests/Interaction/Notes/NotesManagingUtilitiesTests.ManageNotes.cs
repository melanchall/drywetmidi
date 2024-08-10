using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class NotesManagingUtilitiesTests
    {
        #region Nested classes

        private sealed class NotesManagingComparer : TimedObjectsComparer
        {
            public override int Compare(ITimedObject x, ITimedObject y)
            {
                var result = base.Compare(x, y);

                if (x is Note && !(y is Note))
                    return 1;
                else if (!(x is Note) && y is Note)
                    return -1;

                return result;
            }
        }

        #endregion

        #region Test methods

        [Test]
        public void ManageNotes_EmptyCollection() => ManageNotes(
            midiEvents: Array.Empty<MidiEvent>(),
            expectedNotes: Array.Empty<Note>(),
            expectedMidiEventsAfterManaging: Array.Empty<MidiEvent>());

        [Test]
        public void ManageNotes_NoNotes() => ManageNotes(
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
            },
            expectedNotes: Array.Empty<Note>(),
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
            });

        [Test]
        public void ManageNotes_SingleNote() => ManageNotes(
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedNotes: new[]
            {
                new Note((SevenBitNumber)70, 100, 0) { Velocity = SevenBitNumber.MaxValue },
            },
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                new SequenceTrackNameEvent("B"),
            });

        [Test]
        public void ManageNotes_SingleNote_Remove() => ManageNotes(
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedNotes: new[]
            {
                new Note((SevenBitNumber)70, 100, 0) { Velocity = SevenBitNumber.MaxValue },
            },
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
            },
            manage: n => n.Clear());

        [Test]
        public void ManageNotes_MultipleNotes() => ManageNotes(
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { DeltaTime = 50 },
                new TextEvent("C"),
            },
            expectedNotes: new[]
            {
                new Note((SevenBitNumber)70, 100, 0) { Velocity = SevenBitNumber.MaxValue },
                new Note((SevenBitNumber)7, 50, 100) { Velocity = SevenBitNumber.MaxValue },
            },
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                new SequenceTrackNameEvent("B"),
                new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { DeltaTime = 50 },
                new TextEvent("C"),
            });

        [Test]
        public void ManageNotes_MultipleNotes_Process() => ManageNotes(
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { DeltaTime = 50 },
                new TextEvent("C"),
            },
            expectedNotes: new[]
            {
                new Note((SevenBitNumber)70, 100, 0) { Velocity = SevenBitNumber.MaxValue },
                new Note((SevenBitNumber)7, 50, 100) { Velocity = SevenBitNumber.MaxValue },
            },
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)75, (SevenBitNumber)122),
                new NoteOffEvent((SevenBitNumber)75, SevenBitNumber.MinValue) { DeltaTime = 100 },
                new SequenceTrackNameEvent("B"),
                new NoteOnEvent((SevenBitNumber)12, (SevenBitNumber)122),
                new NoteOffEvent((SevenBitNumber)12, SevenBitNumber.MinValue) { DeltaTime = 50 },
                new TextEvent("C"),
            },
            manage: notes =>
            {
                foreach (var n in notes)
                {
                    n.NoteNumber += (SevenBitNumber)5;
                    n.Velocity -= (SevenBitNumber)5;
                }
            });

        [Test]
        public void ManageNotes_CustomNote_1() => ManageNotes(
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { DeltaTime = 50 },
                new TextEvent("C"),
            },
            expectedNotes: new[]
            {
                new CustomNote(
                    new CustomTimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), 0, 0, 1),
                    new CustomTimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 100, 0, 3),
                    0),
                new CustomNote(
                    new CustomTimedEvent(new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue), 100, 0, 4),
                    new CustomTimedEvent(new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue), 150, 0, 5),
                    0),
            },
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                new SequenceTrackNameEvent("B"),
                new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { DeltaTime = 50 },
                new TextEvent("C"),
            },
            noteDetectionSettings: new NoteDetectionSettings
            {
                Constructor = CustomNoteConstructor
            },
            timedEventDetectionSettings: new TimedEventDetectionSettings
            {
                Constructor = CustomTimedEventConstructor
            });

        [Test]
        public void ManageNotes_CustomNote_2() => ManageNotes(
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { DeltaTime = 50 },
                new TextEvent("C"),
            },
            expectedNotes: new[]
            {
                new Note(
                    new CustomTimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), 0, 0, 1),
                    new CustomTimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 100, 0, 3)),
                new Note(
                    new CustomTimedEvent(new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue), 100, 0, 4),
                    new CustomTimedEvent(new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue), 150, 0, 5)),
            },
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                new SequenceTrackNameEvent("B"),
                new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { DeltaTime = 50 },
                new TextEvent("C"),
            },
            timedEventDetectionSettings: new TimedEventDetectionSettings
            {
                Constructor = CustomTimedEventConstructor
            });

        [Test]
        public void ManageNotes_CustomComparer() => ManageNotes(
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { DeltaTime = 50 },
                new TextEvent("C"),
            },
            expectedNotes: new[]
            {
                new Note((SevenBitNumber)70, 100, 0) { Velocity = SevenBitNumber.MaxValue },
                new Note((SevenBitNumber)7, 50, 100) { Velocity = SevenBitNumber.MaxValue },
            },
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue),
                new TextEvent("C") { DeltaTime = 50 },
                new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue),
            },
            timedObjectsComparer: new NotesManagingComparer());

        #endregion

        #region Private methods

        private void ManageNotes(
            ICollection<MidiEvent> midiEvents,
            ICollection<Note> expectedNotes,
            ICollection<MidiEvent> expectedMidiEventsAfterManaging,
            Action<TimedObjectsCollection<Note>> manage = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            TimedObjectsComparer timedObjectsComparer = null)
        {
            var trackChunk = new TrackChunk(midiEvents);
            ManageNotes(
                () => trackChunk.ManageNotes(noteDetectionSettings, timedEventDetectionSettings, timedObjectsComparer),
                "track chunk",
                midiEvents,
                expectedNotes,
                () => trackChunk.Events,
                expectedMidiEventsAfterManaging,
                manage,
                noteDetectionSettings,
                timedEventDetectionSettings,
                timedObjectsComparer);

            var eventsCollection = new TrackChunk(midiEvents).Events;
            ManageNotes(
                () => eventsCollection.ManageNotes(noteDetectionSettings, timedEventDetectionSettings, timedObjectsComparer),
                "events collection",
                midiEvents,
                expectedNotes,
                () => eventsCollection,
                expectedMidiEventsAfterManaging,
                manage,
                noteDetectionSettings,
                timedEventDetectionSettings,
                timedObjectsComparer);
        }

        private void ManageNotes(
            Func<TimedObjectsManager<Note>> getNotesManager,
            string sourceLabel,
            ICollection<MidiEvent> midiEvents,
            ICollection<Note> expectedNotes,
            Func<ICollection<MidiEvent>> getMidiEventsAfterManaging,
            ICollection<MidiEvent> expectedMidiEventsAfterManaging,
            Action<TimedObjectsCollection<Note>> manage = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            TimedObjectsComparer timedObjectsComparer = null)
        {
            var trackChunk = new TrackChunk(midiEvents);

            using (var objectsManager = getNotesManager())
            {
                var notes = objectsManager.Objects;
                MidiAsserts.AreEqual(expectedNotes, notes, $"Invalid notes ({sourceLabel}).");

                manage?.Invoke(notes);
            }

            MidiAsserts.AreEqual(
                expectedMidiEventsAfterManaging,
                getMidiEventsAfterManaging(),
                true,
                $"Invalid events after managing ({sourceLabel}).");
        }

        #endregion
    }
}
