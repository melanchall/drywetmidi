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
        public void GetObjects_ChordsAndNotesAndTimedEvents_FromTimedEvents_UncompletedChord()
        {
            GetObjects_ChordsAndNotesAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 10),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10), 10),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0), 30),
                    new TimedEvent(new NoteOnEvent(), 70),
                    new TimedEvent(new NoteOffEvent(), 80),
                    new TimedEvent(new ProgramChangeEvent(), 90)
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)0, 10, 0) { Velocity = (SevenBitNumber)0 },
                        new Note((SevenBitNumber)70, 20, 10) { Velocity = (SevenBitNumber)10 }),
                    new Note((SevenBitNumber)0, 10, 70) { Velocity = (SevenBitNumber)0 },
                    new TimedEvent(new ProgramChangeEvent(), 90)
                },
                settings: new ChordDetectionSettings
                {
                    NotesTolerance = 20,
                    NotesMinCount = 2
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

        #endregion

        #region Private methods

        private void GetObjects_ChordsAndNotesAndTimedEvents(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects,
            ChordDetectionSettings settings)
        {
            GetObjects(
                inputObjects,
                outputObjects,
                ObjectType.TimedEvent | ObjectType.Note | ObjectType.Chord,
                new ObjectDetectionSettings
                {
                    ChordDetectionSettings = settings
                });
        }

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
            ChordDetectionSettings settings)
        {
            var actualObjects = inputMidiFile
                .GetObjects(
                    ObjectType.TimedEvent | ObjectType.Note | ObjectType.Chord,
                    new ObjectDetectionSettings
                    {
                        ChordDetectionSettings = settings
                    })
                .ToList();

            MidiAsserts.AreEqual(outputObjects, actualObjects, true, 0, "Objects are invalid.");
        }

        #endregion
    }
}
