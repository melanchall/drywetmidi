using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class ChordsManagingUtilitiesTests
    {
        #region Nested classes

        private sealed class ChordsManagingComparer : TimedObjectsComparer
        {
            public override int Compare(ITimedObject x, ITimedObject y)
            {
                var result = base.Compare(x, y);

                if (x is Chord && !(y is Chord))
                    return 1;
                else if (!(x is Chord) && y is Chord)
                    return -1;

                return result;
            }
        }

        #endregion

        #region Test methpds

        [Test]
        public void ManageChords_EmptyCollection() => ManageChords(
            midiEvents: Array.Empty<MidiEvent>(),
            expectedChords: Array.Empty<Chord>(),
            expectedMidiEventsAfterManaging: Array.Empty<MidiEvent>());

        [Test]
        public void ManageChords_NoChords() => ManageChords(
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
            },
            expectedChords: Array.Empty<Chord>(),
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
            });

        [Test]
        public void ManageChords_SingleChord() => ManageChords(
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 100, 0) { Velocity = SevenBitNumber.MaxValue }),
            },
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                new SequenceTrackNameEvent("B"),
            });

        [Test]
        public void ManageChords_SingleChord_Remove() => ManageChords(
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 100, 0) { Velocity = SevenBitNumber.MaxValue }),
            },
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new SequenceTrackNameEvent("B") { DeltaTime = 100 },
            },
            manage: chords => chords.Clear());

        [Test]
        public void ManageChords_MultipleChords() => ManageChords(
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
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 100, 0) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(
                    new Note((SevenBitNumber)7, 50, 100) { Velocity = SevenBitNumber.MaxValue }),
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
        public void ManageChords_MultipleChords_Process() => ManageChords(
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
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 100, 0) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(
                    new Note((SevenBitNumber)7, 50, 100) { Velocity = SevenBitNumber.MaxValue }),
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
            manage: chords =>
            {
                foreach (var n in chords.SelectMany(c => c.Notes))
                {
                    n.NoteNumber += (SevenBitNumber)5;
                    n.Velocity -= (SevenBitNumber)5;
                }
            });

        [Test]
        public void ManageChords_CustomChord_1() => ManageChords(
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
            expectedChords: new[]
            {
                new CustomChord(
                    new[]
                    {
                        new CustomNote(
                            new CustomTimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), 0, 0, 1),
                            new CustomTimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 100, 0, 3),
                            0)
                    },
                    0),
                new CustomChord(
                    new[]
                    {
                        new CustomNote(
                            new CustomTimedEvent(new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue), 100, 0, 4),
                            new CustomTimedEvent(new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue), 150, 0, 5),
                            0)
                    },
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
            chordDetectionSettings: new ChordDetectionSettings
            {
                Constructor = CustomChordConstructor
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
        public void ManageChords_CustomChord_2() => ManageChords(
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
            expectedChords: new[]
            {
                new CustomChord(
                    new[]
                    {
                        new CustomNote(
                            new CustomTimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), 0, 0, 1),
                            new CustomTimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 100, 0, 3),
                            0)
                    },
                    0),
                new CustomChord(
                    new[]
                    {
                        new CustomNote(
                            new CustomTimedEvent(new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue), 100, 0, 4),
                            new CustomTimedEvent(new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue), 150, 0, 5),
                            0)
                    },
                    0),
            },
            expectedMidiEventsAfterManaging: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)107),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                new SequenceTrackNameEvent("B"),
                new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue) { DeltaTime = 50 },
                new TextEvent("C"),
            },
            manage: chords =>
            {
                foreach (var c in chords)
                {
                    var note = (CustomNote)c.Notes.First();
                    var timedEvent = (CustomTimedEvent)note.GetTimedNoteOnEvent();
                    if (timedEvent.EventIndex == 1)
                        note.Velocity -= (SevenBitNumber)20;
                }
            },
            chordDetectionSettings: new ChordDetectionSettings
            {
                Constructor = CustomChordConstructor
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
        public void ManageChords_CustomChord_3() => ManageChords(
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
            expectedChords: new[]
            {
                new Chord(
                    new Note(
                        new CustomTimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), 0, 0, 1),
                        new CustomTimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 100, 0, 3))),
                new Chord(
                    new Note(
                        new CustomTimedEvent(new NoteOnEvent((SevenBitNumber)7, SevenBitNumber.MaxValue), 100, 0, 4),
                        new CustomTimedEvent(new NoteOffEvent((SevenBitNumber)7, SevenBitNumber.MinValue), 150, 0, 5))),
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
        public void ManageChords_CustomComparer() => ManageChords(
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
            expectedChords: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 100, 0) { Velocity = SevenBitNumber.MaxValue }),
                new Chord(
                    new Note((SevenBitNumber)7, 50, 100) { Velocity = SevenBitNumber.MaxValue }),
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
            timedObjectsComparer: new ChordsManagingComparer());

        #endregion

        #region Private methods

        private void ManageChords(
            ICollection<MidiEvent> midiEvents,
            ICollection<Chord> expectedChords,
            ICollection<MidiEvent> expectedMidiEventsAfterManaging,
            Action<TimedObjectsCollection<Chord>> manage = null,
            ChordDetectionSettings chordDetectionSettings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            TimedObjectsComparer timedObjectsComparer = null)
        {
            var trackChunk = new TrackChunk(midiEvents);
            ManageChords(
                () => trackChunk.ManageChords(chordDetectionSettings, noteDetectionSettings, timedEventDetectionSettings, timedObjectsComparer),
                "track chunk",
                midiEvents,
                expectedChords,
                () => trackChunk.Events,
                expectedMidiEventsAfterManaging,
                manage,
                chordDetectionSettings,
                noteDetectionSettings,
                timedEventDetectionSettings,
                timedObjectsComparer);

            var eventsCollection = new TrackChunk(midiEvents).Events;
            ManageChords(
                () => eventsCollection.ManageChords(chordDetectionSettings, noteDetectionSettings, timedEventDetectionSettings, timedObjectsComparer),
                "events collection",
                midiEvents,
                expectedChords,
                () => eventsCollection,
                expectedMidiEventsAfterManaging,
                manage,
                chordDetectionSettings,
                noteDetectionSettings,
                timedEventDetectionSettings,
                timedObjectsComparer);
        }

        private void ManageChords(
            Func<TimedObjectsManager<Chord>> getChordsManager,
            string sourceLabel,
            ICollection<MidiEvent> midiEvents,
            ICollection<Chord> expectedChords,
            Func<ICollection<MidiEvent>> getMidiEventsAfterManaging,
            ICollection<MidiEvent> expectedMidiEventsAfterManaging,
            Action<TimedObjectsCollection<Chord>> manage = null,
            ChordDetectionSettings chordDetectionSettings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            TimedObjectsComparer timedObjectsComparer = null)
        {
            using (var objectsManager = getChordsManager())
            {
                var chords = objectsManager.Objects;
                MidiAsserts.AreEqual(expectedChords, chords, $"Invalid chords ({sourceLabel}).");

                manage?.Invoke(chords);
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
