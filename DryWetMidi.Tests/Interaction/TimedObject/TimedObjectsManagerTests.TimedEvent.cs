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
        public void CheckSingleTypedManager_Generic_TimedEvent_EmptyCollection() => CheckObjectsManager_Generic<TimedEvent>(
            new EventsCollection(),
            manageObjects: objects =>
            {
                objects.Add(new TimedEvent(new TextEvent("A"), 100));
                objects.Add(new TimedEvent(new TextEvent("B"), 1000));
                objects.Add(new TimedEvent(new TextEvent("Ba"), 50));
                objects.Add(
                    new TimedEvent(new NoteOnEvent(), 10),
                    new TimedEvent(new NoteOffEvent(), 5));

                objects.RemoveAll(e => e.Event.EventType == MidiEventType.Text && ((TextEvent)e.Event).Text.StartsWith("B"));
            },
            expectedObjects: new[]
            {
                new TimedEvent(new NoteOffEvent(), 5),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new TextEvent("A"), 100)
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOffEvent(), 5),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new TextEvent("A"), 100)
            });

        [Test]
        public void CheckSingleTypedManager_Generic_TimedEvent_NonEmptyCollection() => CheckObjectsManager_Generic<TimedEvent>(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            manageObjects: objects =>
            {
                objects.Add(new TimedEvent(new TextEvent("A"), 100));

                var x = new TimedEvent(new TextEvent("B"), 1000);
                objects.Add(x);

                var y = new TimedEvent(new TextEvent("Ba"), 50);
                objects.Add(y);
                objects.Add(
                    new TimedEvent(new NoteOnEvent(), 10),
                    new TimedEvent(new NoteOffEvent(), 5));

                objects.Remove(x, y);
            },
            expectedObjects: new[]
            {
                new TimedEvent(new NoteOffEvent(), 5),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new ProgramChangeEvent(), 20),
                new TimedEvent(new TextEvent("A"), 100),
                new TimedEvent(new ControlChangeEvent(), 200)
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOffEvent(), 5),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new ProgramChangeEvent(), 20),
                new TimedEvent(new TextEvent("A"), 100),
                new TimedEvent(new ControlChangeEvent(), 200)
            });

        [Test]
        public void CheckSingleTypedManager_Generic_TimedEvent_NonEmptyCollection_Clear() => CheckObjectsManager_Generic<TimedEvent>(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            manageObjects: objects =>
            {
                objects.Clear();
            },
            expectedObjects: Array.Empty<TimedEvent>(),
            expectedEvents: Array.Empty<TimedEvent>());

        [Test]
        public void CheckSingleTypedManager_Generic_TimedEvent_SameTimeEventComparison_1() => CheckObjectsManager_Generic<TimedEvent>(
            new EventsCollection
            {
                new ProgramChangeEvent(),
                new ControlChangeEvent(),
            },
            manageObjects: objects => { },
            expectedObjects: new[]
            {
                new TimedEvent(new ProgramChangeEvent(), 0),
                new TimedEvent(new ControlChangeEvent(), 0)
            },
            expectedEvents: new[]
            {
                new TimedEvent(new ProgramChangeEvent(), 0),
                new TimedEvent(new ControlChangeEvent(), 0)
            },
            sameTimeEventComparison: (x, y) => x.EventType == MidiEventType.ProgramChange ? -1 : 0);

        [Test]
        public void CheckSingleTypedManager_Generic_TimedEvent_SameTimeEventComparison_2() => CheckObjectsManager_Generic<TimedEvent>(
            new EventsCollection
            {
                new ProgramChangeEvent(),
                new ControlChangeEvent(),
            },
            manageObjects: objects => { },
            expectedObjects: new[]
            {
                new TimedEvent(new ControlChangeEvent(), 0),
                new TimedEvent(new ProgramChangeEvent(), 0)
            },
            expectedEvents: new[]
            {
                new TimedEvent(new ControlChangeEvent(), 0),
                new TimedEvent(new ProgramChangeEvent(), 0)
            },
            sameTimeEventComparison: (x, y) => x.EventType == MidiEventType.ProgramChange ? 1 : 0);

        [Test]
        public void CheckSingleTypedManager_ObjectType_TimedEvent_EmptyCollection() => CheckObjectsManager(
            new EventsCollection(),
            ObjectType.TimedEvent,
            manageObjects: objects =>
            {
                objects.Add(new TimedEvent(new TextEvent("A"), 100));
                objects.Add(new TimedEvent(new TextEvent("B"), 1000));
                objects.Add(new TimedEvent(new TextEvent("Ba"), 50));
                objects.Add(
                    new TimedEvent(new NoteOnEvent(), 10),
                    new TimedEvent(new NoteOffEvent(), 5));

                objects.RemoveAll(obj =>
                {
                    var e = obj as TimedEvent;
                    return e != null && e.Event.EventType == MidiEventType.Text && ((TextEvent)e.Event).Text.StartsWith("B");
                });
            },
            expectedObjects: new[]
            {
                new TimedEvent(new NoteOffEvent(), 5),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new TextEvent("A"), 100)
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOffEvent(), 5),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new TextEvent("A"), 100)
            });

        [Test]
        public void CheckSingleTypedManager_ObjectType_TimedEvent_NonEmptyCollection() => CheckObjectsManager(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            ObjectType.TimedEvent,
            manageObjects: objects =>
            {
                objects.Add(new TimedEvent(new TextEvent("A"), 100));

                var x = new TimedEvent(new TextEvent("B"), 1000);
                objects.Add(x);

                var y = new TimedEvent(new TextEvent("Ba"), 50);
                objects.Add(y);
                objects.Add(
                    new TimedEvent(new NoteOnEvent(), 10),
                    new TimedEvent(new NoteOffEvent(), 5));

                objects.Remove(x, y);
            },
            expectedObjects: new[]
            {
                new TimedEvent(new NoteOffEvent(), 5),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new ProgramChangeEvent(), 20),
                new TimedEvent(new TextEvent("A"), 100),
                new TimedEvent(new ControlChangeEvent(), 200)
            },
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOffEvent(), 5),
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new ProgramChangeEvent(), 20),
                new TimedEvent(new TextEvent("A"), 100),
                new TimedEvent(new ControlChangeEvent(), 200)
            });

        [Test]
        public void CheckSingleTypedManager_ObjectType_TimedEvent_NonEmptyCollection_Clear() => CheckObjectsManager(
            new EventsCollection
            {
                new ProgramChangeEvent { DeltaTime = 20 },
                new ControlChangeEvent { DeltaTime = 180 },
            },
            ObjectType.TimedEvent,
            manageObjects: objects =>
            {
                objects.Clear();
            },
            expectedObjects: Array.Empty<TimedEvent>(),
            expectedEvents: Array.Empty<TimedEvent>());

        #endregion
    }
}
