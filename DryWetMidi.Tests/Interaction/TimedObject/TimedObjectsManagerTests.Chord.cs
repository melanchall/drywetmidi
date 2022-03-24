using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectsManagerTests
    {
        #region Test methods

        [Test]
        public void CheckSingleTypedManager_Generic_Chord_EmptyCollection() => CheckObjectsManager_Generic<Chord>(
            new EventsCollection(),
            manageObjects: objects =>
            {
                objects.Add(new Chord(
                    new Note((SevenBitNumber)60, 50, 10),
                    new Note((SevenBitNumber)61, 50, 12)));
                objects.Add(new Chord(
                    new Note((SevenBitNumber)50, 50, 8)));
                objects.Add(new Chord(
                    new Note((SevenBitNumber)50, 40, 7),
                    new Note((SevenBitNumber)52, 40, 5)));
                objects.Add(
                    new Chord(
                        new Note((SevenBitNumber)45, 10, 19),
                        new Note((SevenBitNumber)55, 10, 25)),
                    new Chord(
                        new Note((SevenBitNumber)25, 3, 15)));

                objects.RemoveAll(c => c.Time < 10);
            },
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)60, 50, 10),
                    new Note((SevenBitNumber)61, 50, 12)),
                new Chord(
                    new Note((SevenBitNumber)25, 3, 15)),
                new Chord(
                    new Note((SevenBitNumber)45, 10, 19),
                    new Note((SevenBitNumber)55, 10, 25))
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)61, Note.DefaultVelocity), 12),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)25, Note.DefaultVelocity), 15),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)25, Note.DefaultOffVelocity), 18),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)45, Note.DefaultVelocity), 19),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)55, Note.DefaultVelocity), 25),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)45, Note.DefaultOffVelocity), 29),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)55, Note.DefaultOffVelocity), 35),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), 60),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)61, Note.DefaultOffVelocity), 62),
            });

        [Test]
        public void CheckSingleTypedManager_Generic_Chord_NonEmptyCollection() => CheckObjectsManager_Generic<Chord>(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
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
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)60, 50, 10),
                    new Note((SevenBitNumber)61, 50, 12)),
                new Chord(
                    new Note((SevenBitNumber)25, 3, 15)),
                new Chord(
                    new Note((SevenBitNumber)45, 10, 19),
                    new Note((SevenBitNumber)55, 10, 25))
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
        public void CheckSingleTypedManager_Generic_Chord_NonEmptyCollection_Clear() => CheckObjectsManager_Generic<Chord>(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            manageObjects: objects =>
            {
                objects.Clear();
            },
            expectedObjects: Array.Empty<Chord>(),
            expectedEvents: new[]
            {
                new TimedEvent(new ProgramChangeEvent(), 20),
                new TimedEvent(new ControlChangeEvent(), 200)
            });

        [Test]
        public void CheckSingleTypedManager_Generic_Chord_Settings_1Note() => CheckObjectsManager_Generic<Chord>(
            new EventsCollection
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0) { DeltaTime = 20 }
            },
            manageObjects: objects => { },
            expectedObjects: new[]
            {
                new Chord(new Note((SevenBitNumber)0, 10, 0) { Velocity = (SevenBitNumber)0 }),
                new Chord(new Note((SevenBitNumber)70, 20, 10) { Velocity = (SevenBitNumber)10 }),
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent(), 0),
                new TimedEvent(new NoteOffEvent(), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0), 30)
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 1
                }
            });

        [Test]
        public void CheckSingleTypedManager_Generic_Chord_Settings_2Note() => CheckObjectsManager_Generic<Chord>(
            new EventsCollection
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0) { DeltaTime = 20 }
            },
            manageObjects: objects => { },
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)0, 10, 0) { Velocity = (SevenBitNumber)0 },
                    new Note((SevenBitNumber)70, 20, 10) { Velocity = (SevenBitNumber)10 }),
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent(), 0),
                new TimedEvent(new NoteOffEvent(), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0), 30)
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2,
                    NotesTolerance = 20
                }
            });

        [Test]
        public void CheckSingleTypedManager_ObjectType_Chord_EmptyCollection() => CheckObjectsManager(
            new EventsCollection(),
            ObjectType.Chord,
            manageObjects: objects =>
            {
                objects.Add(new Chord(
                    new Note((SevenBitNumber)60, 50, 10),
                    new Note((SevenBitNumber)61, 50, 12)));
                objects.Add(new Chord(
                    new Note((SevenBitNumber)50, 50, 8)));
                objects.Add(new Chord(
                    new Note((SevenBitNumber)50, 40, 7),
                    new Note((SevenBitNumber)52, 40, 5)));
                objects.Add(
                    new Chord(
                        new Note((SevenBitNumber)45, 10, 19),
                        new Note((SevenBitNumber)55, 10, 25)),
                    new Chord(
                        new Note((SevenBitNumber)25, 3, 15)));

                objects.RemoveAll(obj => obj.Time < 10);
            },
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)60, 50, 10),
                    new Note((SevenBitNumber)61, 50, 12)),
                new Chord(
                    new Note((SevenBitNumber)25, 3, 15)),
                new Chord(
                    new Note((SevenBitNumber)45, 10, 19),
                    new Note((SevenBitNumber)55, 10, 25))
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)61, Note.DefaultVelocity), 12),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)25, Note.DefaultVelocity), 15),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)25, Note.DefaultOffVelocity), 18),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)45, Note.DefaultVelocity), 19),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)55, Note.DefaultVelocity), 25),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)45, Note.DefaultOffVelocity), 29),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)55, Note.DefaultOffVelocity), 35),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), 60),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)61, Note.DefaultOffVelocity), 62),
            });

        [Test]
        public void CheckSingleTypedManager_ObjectType_Chord_NonEmptyCollection() => CheckObjectsManager(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            ObjectType.Chord,
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
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)60, 50, 10),
                    new Note((SevenBitNumber)61, 50, 12)),
                new Chord(
                    new Note((SevenBitNumber)25, 3, 15)),
                new Chord(
                    new Note((SevenBitNumber)45, 10, 19),
                    new Note((SevenBitNumber)55, 10, 25))
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
        public void CheckSingleTypedManager_ObjectType_Chord_NonEmptyCollection_Clear() => CheckObjectsManager(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            ObjectType.Chord,
            manageObjects: objects =>
            {
                objects.Clear();
            },
            expectedObjects: Array.Empty<Chord>(),
            expectedEvents: new[]
            {
                new TimedEvent(new ProgramChangeEvent(), 20),
                new TimedEvent(new ControlChangeEvent(), 200)
            });

        [Test]
        public void CheckSingleTypedManager_ObjectType_Chord_Settings_1Note() => CheckObjectsManager(
            new EventsCollection
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0) { DeltaTime = 20 }
            },
            ObjectType.Chord,
            manageObjects: objects => { },
            expectedObjects: new[]
            {
                new Chord(new Note((SevenBitNumber)0, 10, 0) { Velocity = (SevenBitNumber)0 }),
                new Chord(new Note((SevenBitNumber)70, 20, 10) { Velocity = (SevenBitNumber)10 }),
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent(), 0),
                new TimedEvent(new NoteOffEvent(), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0), 30)
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 1
                }
            });

        [Test]
        public void CheckSingleTypedManager_ObjectType_Chord_Settings_2Note() => CheckObjectsManager(
            new EventsCollection
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10),
                new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0) { DeltaTime = 20 }
            },
            ObjectType.Chord,
            manageObjects: objects => { },
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)0, 10, 0) { Velocity = (SevenBitNumber)0 },
                    new Note((SevenBitNumber)70, 20, 10) { Velocity = (SevenBitNumber)10 }),
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent(), 0),
                new TimedEvent(new NoteOffEvent(), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)10), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0), 30)
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
