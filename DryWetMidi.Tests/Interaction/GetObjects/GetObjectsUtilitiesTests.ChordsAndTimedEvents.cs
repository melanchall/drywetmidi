using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class GetObjectsUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetObjects_ChordsAndTimedEvents_FromNotes_SingleNote()
        {
            GetObjects_ChordsAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50)),
                });
        }

        [Test]
        public void GetObjects_ChordsAndTimedEvents_FromNotes_MultipleNotes_SameTime()
        {
            GetObjects_ChordsAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50),
                    new Note((SevenBitNumber)70),
                    new Note((SevenBitNumber)90, 100, 0),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50),
                        new Note((SevenBitNumber)70),
                        new Note((SevenBitNumber)90, 100, 0)),
                });
        }

        [TestCase(0)]
        [TestCase(10)]
        public void GetObjects_ChordsAndTimedEvents_FromNotes_MultipleNotes_ExceedingNotesTolerance(long notesTolerance)
        {
            GetObjects_ChordsAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50),
                    new Note((SevenBitNumber)70),
                    new Note((SevenBitNumber)90, 100, notesTolerance + 1),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50),
                        new Note((SevenBitNumber)70)),
                    new Chord(
                        new Note((SevenBitNumber)90, 100, notesTolerance + 1)),
                },
                notesTolerance: notesTolerance);
        }

        [Test]
        public void GetObjects_ChordsAndTimedEvents_FromNotes_MultipleNotes_DifferentChannels()
        {
            GetObjects_ChordsAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50),
                    new Note((SevenBitNumber)70),
                    new Note((SevenBitNumber)90) { Channel = (FourBitNumber)1 },
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50),
                        new Note((SevenBitNumber)70)),
                    new Chord(
                        new Note((SevenBitNumber)90) { Channel = (FourBitNumber)1 }),
                });
        }

        [Test]
        public void GetObjects_ChordsAndTimedEvents_FromNotesAndTimedEvents_SingleNote()
        {
            GetObjects_ChordsAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50),
                    new TimedEvent(new TextEvent("A"), 40),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50)),
                    new TimedEvent(new TextEvent("A"), 40),
                });
        }

        [Test]
        public void GetObjects_ChordsAndTimedEvents_FromNotesAndTimedEvents_MultipleNotes_SameTime()
        {
            GetObjects_ChordsAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 10),
                    new Note((SevenBitNumber)50),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new TextEvent("C"), 30),
                    new Note((SevenBitNumber)70),
                    new Note((SevenBitNumber)90, 100, 0),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50),
                        new Note((SevenBitNumber)70),
                        new Note((SevenBitNumber)90, 100, 0)),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new TextEvent("C"), 30),
                });
        }

        [TestCase(0)]
        [TestCase(10)]
        public void GetObjects_ChordsAndTimedEvents_FromNotesAndTimedEvents_MultipleNotes_ExceedingNotesTolerance(long notesTolerance)
        {
            GetObjects_ChordsAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 100),
                    new Note((SevenBitNumber)50),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new TextEvent("C"), 30),
                    new Note((SevenBitNumber)70),
                    new Note((SevenBitNumber)90, 100, notesTolerance + 1),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50),
                        new Note((SevenBitNumber)70)),
                    new TimedEvent(new TextEvent("B"), 0),
                    new Chord(
                        new Note((SevenBitNumber)90, 100, notesTolerance + 1)),
                    new TimedEvent(new TextEvent("C"), 30),
                    new TimedEvent(new TextEvent("A"), 100),
                },
                notesTolerance: notesTolerance);
        }

        [Test]
        public void GetObjects_ChordsAndTimedEvents_FromNotesAndTimedEvents_MultipleNotes_DifferentChannels()
        {
            GetObjects_ChordsAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 10),
                    new Note((SevenBitNumber)50),
                    new Note((SevenBitNumber)70),
                    new TimedEvent(new TextEvent("B"), 0),
                    new Note((SevenBitNumber)90) { Channel = (FourBitNumber)1 },
                    new TimedEvent(new TextEvent("C"), 30),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50),
                        new Note((SevenBitNumber)70)),
                    new TimedEvent(new TextEvent("B"), 0),
                    new Chord(
                        new Note((SevenBitNumber)90) { Channel = (FourBitNumber)1 }),
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new TextEvent("C"), 30),
                });
        }

        [Test]
        public void GetObjects_ChordsAndTimedEvents_FromTimedEvents_SameTime()
        {
            GetObjects_ChordsAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 0),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), 30),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 40),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new TextEvent("C"), 30),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 0),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)90, SevenBitNumber.MinValue), 100),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50, 30, 0),
                        new Note((SevenBitNumber)70, 40, 0),
                        new Note((SevenBitNumber)90, 100, 0)),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new TextEvent("C"), 30),
                });
        }

        [TestCase(0)]
        [TestCase(10)]
        public void GetObjects_ChordsAndTimedEvents_FromTimedEvents_ExceedingNotesTolerance(long notesTolerance)
        {
            GetObjects_ChordsAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 100),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 0),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), 30),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), notesTolerance + 1),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), notesTolerance + 1 + 40),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new TextEvent("C"), 30),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 0),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)90, SevenBitNumber.MinValue), 100),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50, 30, 0),
                        new Note((SevenBitNumber)90, 100, 0)),
                    new TimedEvent(new TextEvent("B"), 0),
                    new Chord(
                        new Note((SevenBitNumber)70, 40, notesTolerance + 1)),
                    new TimedEvent(new TextEvent("C"), 30),
                    new TimedEvent(new TextEvent("A"), 100),
                },
                notesTolerance: notesTolerance);
        }

        [Test]
        public void GetObjects_ChordsAndTimedEvents_FromTimedEvents_DifferentChannels()
        {
            GetObjects_ChordsAndTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 10),
                    new Note((SevenBitNumber)50),
                    new Note((SevenBitNumber)70),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity) { Channel = (FourBitNumber)1 }, 0),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)90, SevenBitNumber.MinValue) { Channel = (FourBitNumber)1 }, 40),
                    new TimedEvent(new TextEvent("C"), 30),
                },
                outputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50),
                        new Note((SevenBitNumber)70)),
                    new TimedEvent(new TextEvent("B"), 0),
                    new Chord(
                        new Note((SevenBitNumber)90, 40, 0) { Channel = (FourBitNumber)1 }),
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new TextEvent("C"), 30),
                });
        }

        [Test]
        public void GetObjects_ChordsAndTimedEvents_FromTimedEvents_SameTime_UncompletedNote()
        {
            GetObjects_ChordsAndTimedEvents(
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
                        new Note((SevenBitNumber)50, 30),
                        new Note((SevenBitNumber)70, 40)),
                    new TimedEvent(new TextEvent("B"), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 0),
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new TextEvent("C"), 30),
                    new TimedEvent(new TextEvent("D"), 30),
                });
        }

        [Test]
        public void GetObjects_ChordsAndTimedEvents_FromNotesAndChords_1() => GetObjects_ChordsAndTimedEvents(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 100, 30),
                new Chord(
                    new Note((SevenBitNumber)80, 100, 0),
                    new Note((SevenBitNumber)90, 100, 10))
            },
            outputObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)80, 100, 0),
                    new Note((SevenBitNumber)90, 100, 10)),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 30),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 130),
            },
            notesTolerance: 10,
            notesMinCount: 2);

        [Test]
        public void GetObjects_ChordsAndTimedEvents_FromNotesAndChords_2() => GetObjects_ChordsAndTimedEvents(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)50, 50, 10),
                new Note((SevenBitNumber)70, 100, 30),
                new Chord(
                    new Note((SevenBitNumber)80, 100, 0),
                    new Note((SevenBitNumber)90, 100, 10)),
            },
            outputObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)80, 100, 0),
                    new Note((SevenBitNumber)90, 100, 10)),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 30),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), 60),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 130),
            },
            notesTolerance: 10,
            notesMinCount: 2);

        #endregion

        #region Private methods

        private void GetObjects_ChordsAndTimedEvents(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects,
            long notesTolerance = 0,
            int notesMinCount = 1)
        {
            GetObjects(
                inputObjects,
                outputObjects,
                ObjectType.TimedEvent | ObjectType.Chord,
                new ObjectDetectionSettings
                {
                    ChordDetectionSettings = new ChordDetectionSettings
                    {
                        NotesTolerance = notesTolerance,
                        NotesMinCount = notesMinCount
                    }
                });
        }

        #endregion
    }
}
