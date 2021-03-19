using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    // TODO: notes min count tests
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

        #endregion
    }
}
