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
        public void CheckSingleTypedManager_Generic_Note_EmptyCollection() => CheckObjectsManager_Generic<Note>(
            new EventsCollection(),
            manageObjects: objects =>
            {
                objects.Add(new Note((SevenBitNumber)60, 50, 10));
                objects.Add(new Note((SevenBitNumber)50, 50, 8));
                objects.Add(new Note((SevenBitNumber)50, 40, 7));
                objects.Add(
                    new Note((SevenBitNumber)45, 10, 20),
                    new Note((SevenBitNumber)25, 3, 15));

                objects.RemoveAll(n => n.Time < 10);
            },
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)60, 50, 10),
                new Note((SevenBitNumber)25, 3, 15),
                new Note((SevenBitNumber)45, 10, 20)
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)25, Note.DefaultVelocity), 15),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)25, Note.DefaultOffVelocity), 18),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)45, Note.DefaultVelocity), 20),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)45, Note.DefaultOffVelocity), 30),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), 60),
            });

        [Test]
        public void CheckSingleTypedManager_Generic_Note_NonEmptyCollection() => CheckObjectsManager_Generic<Note>(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
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
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)60, 50, 10),
                new Note((SevenBitNumber)25, 3, 15),
                new Note((SevenBitNumber)45, 10, 20)
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
        public void CheckSingleTypedManager_Generic_Note_NonEmptyCollection_Clear() => CheckObjectsManager_Generic<Note>(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            manageObjects: objects =>
            {
                objects.Clear();
            },
            expectedObjects: Array.Empty<Note>(),
            expectedEvents: new[]
            {
                new TimedEvent(new ProgramChangeEvent(), 20),
                new TimedEvent(new ControlChangeEvent(), 200)
            });

        [Test]
        public void CheckSingleTypedManager_Generic_Note_Settings_FirstNoteOn() => CheckObjectsManager_Generic<Note>(
            new EventsCollection
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 100 },
                new NoteOffEvent { DeltaTime = 1000 }
            },
            manageObjects: objects =>
            {
            },
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)0, 110, 0) { Velocity = (SevenBitNumber)0 },
                new Note((SevenBitNumber)0, 1100, 10) { Velocity = (SevenBitNumber)0 }
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent(), 0),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new NoteOffEvent(), 110),
                new TimedEvent(new NoteOffEvent(), 1110),
            },
            settings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn
                }
            });

        [Test]
        public void CheckSingleTypedManager_Generic_Note_Settings_LastNoteOn() => CheckObjectsManager_Generic<Note>(
            new EventsCollection
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 100 },
                new NoteOffEvent { DeltaTime = 1000 }
            },
            manageObjects: objects =>
            {
            },
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)0, 1110, 0) { Velocity = (SevenBitNumber)0 },
                new Note((SevenBitNumber)0, 100, 10) { Velocity = (SevenBitNumber)0 }
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent(), 0),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new NoteOffEvent(), 110),
                new TimedEvent(new NoteOffEvent(), 1110),
            },
            settings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                }
            });

        [Test]
        public void CheckSingleTypedManager_ObjectType_Note_EmptyCollection() => CheckObjectsManager(
            new EventsCollection(),
            ObjectType.Note,
            manageObjects: objects =>
            {
                objects.Add(new Note((SevenBitNumber)60, 50, 10));
                objects.Add(new Note((SevenBitNumber)50, 50, 8));
                objects.Add(new Note((SevenBitNumber)50, 40, 7));
                objects.Add(
                    new Note((SevenBitNumber)45, 10, 20),
                    new Note((SevenBitNumber)25, 3, 15));

                objects.RemoveAll(obj => obj.Time < 10);
            },
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)60, 50, 10),
                new Note((SevenBitNumber)25, 3, 15),
                new Note((SevenBitNumber)45, 10, 20)
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)25, Note.DefaultVelocity), 15),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)25, Note.DefaultOffVelocity), 18),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)45, Note.DefaultVelocity), 20),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)45, Note.DefaultOffVelocity), 30),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)60, Note.DefaultOffVelocity), 60),
            });

        [Test]
        public void CheckSingleTypedManager_ObjectType_Note_NonEmptyCollection() => CheckObjectsManager(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            ObjectType.Note,
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

                objects.RemoveAll(obj => obj.Time < 10);
            },
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)60, 50, 10),
                new Note((SevenBitNumber)25, 3, 15),
                new Note((SevenBitNumber)45, 10, 20)
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
        public void CheckSingleTypedManager_ObjectType_Note_NonEmptyCollection_Clear() => CheckObjectsManager(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            ObjectType.Note,
            manageObjects: objects =>
            {
                objects.Clear();
            },
            expectedObjects: Array.Empty<Note>(),
            expectedEvents: new[]
            {
                new TimedEvent(new ProgramChangeEvent(), 20),
                new TimedEvent(new ControlChangeEvent(), 200)
            });

        [Test]
        public void CheckSingleTypedManager_ObjectType_Note_Settings_FirstNoteOn() => CheckObjectsManager(
            new EventsCollection
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 100 },
                new NoteOffEvent { DeltaTime = 1000 }
            },
            ObjectType.Note,
            manageObjects: objects =>
            {
            },
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)0, 110, 0) { Velocity = (SevenBitNumber)0 },
                new Note((SevenBitNumber)0, 1100, 10) { Velocity = (SevenBitNumber)0 }
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent(), 0),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new NoteOffEvent(), 110),
                new TimedEvent(new NoteOffEvent(), 1110),
            },
            settings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn
                }
            });

        [Test]
        public void CheckSingleTypedManager_ObjectType_Note_Settings_LastNoteOn() => CheckObjectsManager(
            new EventsCollection
            {
                new NoteOnEvent(),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 100 },
                new NoteOffEvent { DeltaTime = 1000 }
            },
            ObjectType.Note,
            manageObjects: objects =>
            {
            },
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)0, 1110, 0) { Velocity = (SevenBitNumber)0 },
                new Note((SevenBitNumber)0, 100, 10) { Velocity = (SevenBitNumber)0 }
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent(), 0),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new NoteOffEvent(), 110),
                new TimedEvent(new NoteOffEvent(), 1110),
            },
            settings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                }
            });

        #endregion
    }
}
