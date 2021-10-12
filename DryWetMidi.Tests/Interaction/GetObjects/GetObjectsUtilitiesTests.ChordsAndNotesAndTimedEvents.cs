using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class GetObjectsUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetObjects_ChordsAndNotesAndTimedEvents_FromTimedEvents_SameTime_UncompletedNote()
        {
            GetObjects_ChordsAndNotesAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 0),
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), 30),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new TextEvent("C"), 30),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 40),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 0),
                    new TimedEvent(new TextEvent("D"), 30),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50, 30, 0),
                        new Note((SevenBitNumber)70, 40, 0)),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 0),
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new TextEvent("C"), 30),
                    new TimedEvent(new TextEvent("D"), 30),
                });
        }

        [Test]
        public void GetObjects_ChordsAndNotesAndTimedEvents_FromTimedEvents_SameTime_AllNotesUncompleted()
        {
            GetObjects_ChordsAndNotesAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 0),
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 0),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new TextEvent("C"), 30),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 0),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new TextEvent("C"), 30),
                });
        }

        [Test]
        public void GetObjects_ChordsAndNotesAndTimedEvents_MidiFile()
        {
            GetObjects_ChordsAndNotesAndTimedEvents(
                inputMidiFile: new MidiFile(
                    new TrackChunk(
                        new TextEvent("1"),
                        new NoteOnEvent { DeltaTime = 1 },
                        new TextEvent("2") { DeltaTime = 1 },
                        new NoteOffEvent { DeltaTime = 1 },
                        new TextEvent("3") { DeltaTime = 1 },
                        new NoteOnEvent { DeltaTime = 1 },
                        new TextEvent("4") { DeltaTime = 1 },
                        new NoteOffEvent { DeltaTime = 1 }),
                    new TrackChunk(
                        new TextEvent("A"),
                        new TextEvent("B") { DeltaTime = 1 },
                        new TextEvent("C") { DeltaTime = 1 },
                        new TextEvent("D") { DeltaTime = 1 },
                        new TextEvent("E") { DeltaTime = 1 },
                        new NoteOnEvent { DeltaTime = 1 },
                        new TextEvent("F") { DeltaTime = 1 },
                        new NoteOffEvent { DeltaTime = 1 })),
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("1"), 0),
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)0, 2, 1) { Velocity = (SevenBitNumber)0 },
                    new TimedEvent(new TextEvent("B") { DeltaTime = 1 }, 1),
                    new TimedEvent(new TextEvent("2") { DeltaTime = 1 }, 2),
                    new TimedEvent(new TextEvent("C") { DeltaTime = 1 }, 2),
                    new TimedEvent(new TextEvent("D") { DeltaTime = 1 }, 3),
                    new TimedEvent(new TextEvent("3") { DeltaTime = 1 }, 4),
                    new TimedEvent(new TextEvent("E") { DeltaTime = 1 }, 4),
                    new Chord(
                        new Note((SevenBitNumber)0, 2, 5) { Velocity = (SevenBitNumber)0 },
                        new Note((SevenBitNumber)0, 2, 5) { Velocity = (SevenBitNumber)0 }),
                    new TimedEvent(new TextEvent("4") { DeltaTime = 1 }, 6),
                    new TimedEvent(new TextEvent("F") { DeltaTime = 1 }, 6),
                },
                chordDetectionSettings: new ChordDetectionSettings
                {
                    ChordSearchContext = ChordSearchContext.AllEventsCollections,
                    NotesMinCount = 2
                });
        }

        #endregion

        #region Private methods

        private void GetObjects_ChordsAndNotesAndTimedEvents(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects,
            long notesTolerance = 0)
        {
            GetObjects(
                inputObjects,
                outputObjects,
                ObjectType.TimedEvent | ObjectType.Note | ObjectType.Chord,
                new ObjectDetectionSettings
                {
                    ChordDetectionSettings = new ChordDetectionSettings
                    {
                        NotesTolerance = notesTolerance
                    }
                });
        }

        private void GetObjects_ChordsAndNotesAndTimedEvents(
            MidiFile inputMidiFile,
            IEnumerable<ITimedObject> outputObjects,
            ChordDetectionSettings chordDetectionSettings)
        {
            var actualObjects = inputMidiFile
                .GetObjects(
                    ObjectType.TimedEvent | ObjectType.Note | ObjectType.Chord,
                    new ObjectDetectionSettings
                    {
                        ChordDetectionSettings = chordDetectionSettings
                    })
                .ToList();

            MidiAsserts.AreEqual(outputObjects, actualObjects, true, 0, "Objects are invalid.");
        }

        #endregion
    }
}
