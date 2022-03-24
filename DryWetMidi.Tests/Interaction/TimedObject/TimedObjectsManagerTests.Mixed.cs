using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectsManagerTests
    {
        #region Test methods

        [Test]
        public void CheckMixedTypedManager_TimedEventAndNote() => CheckObjectsManager(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            ObjectType.TimedEvent | ObjectType.Note,
            manageObjects: objects =>
            {
                objects.Add(new Note((SevenBitNumber)60, 50, 10));

                var x = new Note((SevenBitNumber)50, 50, 8);
                objects.Add(x);

                var y = new Note((SevenBitNumber)50, 40, 7);
                objects.Add(y);
                objects.Add(
                    new Note((SevenBitNumber)45, 10, 20),
                    new Note((SevenBitNumber)25, 3, 15));

                objects.RemoveAll(n => n.Time < 10);
            },
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)60, 50, 10),
                new Note((SevenBitNumber)25, 3, 15),
                new TimedEvent(new ProgramChangeEvent(), 20),
                new Note((SevenBitNumber)45, 10, 20),
                new TimedEvent(new ControlChangeEvent(), 200)
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)25, Note.DefaultVelocity), 15),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)25, Note.DefaultOffVelocity), 18),
                new TimedEvent(new ProgramChangeEvent(), 20),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)45, Note.DefaultVelocity), 20),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)45, Note.DefaultOffVelocity), 30),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), 60),
                new TimedEvent(new ControlChangeEvent(), 200)
            });

        [Test]
        public void CheckMixedTypedManager_TimedEventAndChord() => CheckObjectsManager(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            ObjectType.TimedEvent | ObjectType.Chord,
            manageObjects: objects =>
            {
                objects.Add(new Chord(
                    new Note((SevenBitNumber)60, 50, 10),
                    new Note((SevenBitNumber)61, 50, 12)));

                var x = new Chord(
                    new Note((SevenBitNumber)50, 50, 8));
                objects.Add(x);

                var y = new Chord(
                    new Note((SevenBitNumber)50, 40, 7),
                    new Note((SevenBitNumber)52, 40, 5));
                objects.Add(y);
                objects.Add(
                    new Chord(
                        new Note((SevenBitNumber)45, 10, 19),
                        new Note((SevenBitNumber)55, 10, 25)),
                    new Chord(
                        new Note((SevenBitNumber)25, 3, 15)));

                objects.Remove(x, y);
            },
            expectedObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)60, 50, 10),
                    new Note((SevenBitNumber)61, 50, 12)),
                new Chord(
                    new Note((SevenBitNumber)25, 3, 15)),
                new Chord(
                    new Note((SevenBitNumber)45, 10, 19),
                    new Note((SevenBitNumber)55, 10, 25)),
                new TimedEvent(new ProgramChangeEvent(), 20),
                new TimedEvent(new ControlChangeEvent(), 200)
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)61, Note.DefaultVelocity), 12),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)25, Note.DefaultVelocity), 15),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)25, Note.DefaultOffVelocity), 18),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)45, Note.DefaultVelocity), 19),
                new TimedEvent(new ProgramChangeEvent(), 20),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)55, Note.DefaultVelocity), 25),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)45, Note.DefaultOffVelocity), 29),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)55, Note.DefaultOffVelocity), 35),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), 60),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)61, Note.DefaultOffVelocity), 62),
                new TimedEvent(new ControlChangeEvent(), 200)
            });

        [Test]
        public void CheckMixedTypedManager_NoteAndChord_Settings_2Note() => CheckObjectsManager(
            new EventsCollection
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0) { DeltaTime = 20 },
                new NoteOnEvent { DeltaTime = 40 },
                new NoteOffEvent { DeltaTime = 10 },
                new ProgramChangeEvent { DeltaTime = 10 }
            },
            ObjectType.Note | ObjectType.Chord,
            manageObjects: objects => { },
            expectedObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)0, 10, 0) { Velocity = (SevenBitNumber)0 },
                    new Note((SevenBitNumber)70, 20, 10) { Velocity = (SevenBitNumber)10 }),
                new Note((SevenBitNumber)0, 10, 70) { Velocity = (SevenBitNumber)0 }
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent(), 0),
                new TimedEvent(new NoteOffEvent(), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0), 30),
                new TimedEvent(new NoteOnEvent(), 70),
                new TimedEvent(new NoteOffEvent(), 80),
                new TimedEvent(new ProgramChangeEvent(), 90)
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2,
                    NotesTolerance = 20
                }
            });

        #endregion
    }
}
