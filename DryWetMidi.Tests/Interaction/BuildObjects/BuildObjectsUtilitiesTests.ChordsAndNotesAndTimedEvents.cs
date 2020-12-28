using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class BuildObjectsUtilitiesTests
    {
        #region Test methods

        [Test]
        public void BuildChordsAndNotesAndTimedEvents_FromTimedEvents_SameTime_UncompletedNote()
        {
            CheckBuildingChordsAndNotesAndTimedEvents(
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
                    new Note((SevenBitNumber)50, 30, 0),
                    new Note((SevenBitNumber)70, 40, 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 0),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new TextEvent("C"), 30),
                    new TimedEvent(new TextEvent("D"), 30),
                });
        }

        [Test]
        public void BuildChordsAndNotesAndTimedEvents_FromTimedEvents_SameTime_AllNotesUncompleted()
        {
            CheckBuildingChordsAndNotesAndTimedEvents(
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

        private void CheckBuildingChordsAndNotesAndTimedEvents(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects,
            long notesTolerance = 0)
        {
            CheckObjectsBuilding(
                inputObjects,
                outputObjects,
                ObjectType.TimedEvent | ObjectType.Note | ObjectType.Chord,
                new ObjectsBuildingSettings
                {
                    ChordBuilderSettings = new ChordBuilderSettings
                    {
                        NotesTolerance = notesTolerance
                    }
                });
        }

        #endregion
    }
}
