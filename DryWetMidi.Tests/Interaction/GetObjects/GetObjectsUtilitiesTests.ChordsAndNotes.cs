using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class GetObjectsUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetObjects_ChordsAndNotes_FromTimedEvents_UncompletedChord()
        {
            GetObjects_ChordsAndNotes(
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
                    new Note((SevenBitNumber)0, 10, 70) { Velocity = (SevenBitNumber)0 }
                },
                settings: new ChordDetectionSettings
                {
                    NotesTolerance = 20,
                    NotesMinCount = 2
                });
        }

        #endregion

        #region Private methods

        private void GetObjects_ChordsAndNotes(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects,
            ChordDetectionSettings settings = null)
        {
            GetObjects(
                inputObjects,
                outputObjects,
                ObjectType.Note | ObjectType.Chord,
                new ObjectDetectionSettings
                {
                    ChordDetectionSettings = settings
                });
        }

        #endregion
    }
}
