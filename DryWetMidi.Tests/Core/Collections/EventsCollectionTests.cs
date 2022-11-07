using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class EventsCollectionTests
    {
        #region Test methods

        [Test]
        public void CheckTempoMapProperties_EmptyCollection() => CheckTempoMapProperties(
            changeCollection: e => { },
            isInitialState: true,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_Add_1() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.Add(new TextEvent("A"));
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_Add_2() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.Add(new SetTempoEvent());
            },
            isInitialState: false,
            hasTempoMapEvents: true,
            lastTempoMapEventIndex: 0);

        [Test]
        public void CheckTempoMapProperties_Add_3() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.Add(new TextEvent("A"));
                e.Add(new TimeSignatureEvent());
            },
            isInitialState: false,
            hasTempoMapEvents: true,
            lastTempoMapEventIndex: 1);

        [Test]
        public void CheckTempoMapProperties_AddRange_1() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                });
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_AddRange_2() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                });
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_AddRange_3() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                });
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_AddRange_4() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new SetTempoEvent(),
                    new NoteOffEvent(),
                });
            },
            isInitialState: false,
            hasTempoMapEvents: true,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_AddRange_5() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new SetTempoEvent(),
                    new TimeSignatureEvent(),
                    new NoteOffEvent(),
                });
            },
            isInitialState: false,
            hasTempoMapEvents: true,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_Insert_1() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.Add(new TextEvent());
                e.Insert(0, new NoteOnEvent());
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_Insert_2() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.Insert(0, new NoteOnEvent());
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_Insert_3() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                });
                e.Insert(1, new SetTempoEvent());
            },
            isInitialState: false,
            hasTempoMapEvents: true,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_Insert_4() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                });
                e.Insert(0, new TimeSignatureEvent());
            },
            isInitialState: false,
            hasTempoMapEvents: true,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_InsertRange_1() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.Add(new TextEvent());
                e.InsertRange(0, new MidiEvent[]
                {
                    new NoteOnEvent(),
                });
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_InsertRange_2() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                });
                e.InsertRange(1, new MidiEvent[]
                {
                    new TextEvent(),
                });
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_InsertRange_3() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                });
                e.InsertRange(1, new MidiEvent[]
                {
                    new TextEvent(),
                    new SetTempoEvent(),
                });
            },
            isInitialState: false,
            hasTempoMapEvents: true,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_InsertRange_4() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                });
                e.InsertRange(0, new MidiEvent[]
                {
                    new TextEvent(),
                    new TimeSignatureEvent(),
                });
            },
            isInitialState: false,
            hasTempoMapEvents: true,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_Remove_1() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                });
                e.Remove(new SetTempoEvent());
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_Remove_2()
        {
            var midiEvent = new TextEvent("A");
            CheckTempoMapProperties(
                changeCollection: e =>
                {
                    e.AddRange(new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        midiEvent,
                        new NoteOffEvent(),
                    });
                    e.Remove(midiEvent);
                },
                isInitialState: false,
                hasTempoMapEvents: false,
                lastTempoMapEventIndex: -1);
        }

        [Test]
        public void CheckTempoMapProperties_Remove_3()
        {
            var midiEvent = new TimeSignatureEvent();
            CheckTempoMapProperties(
                changeCollection: e =>
                {
                    e.AddRange(new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        midiEvent,
                        new NoteOffEvent(),
                    });
                    e.Remove(midiEvent);
                },
                isInitialState: false,
                hasTempoMapEvents: false,
                lastTempoMapEventIndex: -1);
        }

        [Test]
        public void CheckTempoMapProperties_Remove_4()
        {
            var midiEvent = new TimeSignatureEvent();
            CheckTempoMapProperties(
                changeCollection: e =>
                {
                    e.AddRange(new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new SetTempoEvent(),
                        midiEvent,
                        new NoteOffEvent(),
                    });
                    e.Remove(midiEvent);
                },
                isInitialState: false,
                hasTempoMapEvents: true,
                lastTempoMapEventIndex: -1);
        }

        [Test]
        public void CheckTempoMapProperties_Remove_5() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new SetTempoEvent(),
                    new NoteOffEvent(),
                });
                e.Remove(new SetTempoEvent());
            },
            isInitialState: false,
            hasTempoMapEvents: true,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_RemoveAt_1() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                });
                e.RemoveAt(0);
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_RemoveAt_2() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                });
                e.RemoveAt(1);
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_RemoveAt_3() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TimeSignatureEvent(),
                    new NoteOffEvent(),
                });
                e.RemoveAt(1);
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_RemoveAt_4() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new SetTempoEvent(),
                    new TimeSignatureEvent(),
                    new NoteOffEvent(),
                });
                e.RemoveAt(2);
            },
            isInitialState: false,
            hasTempoMapEvents: true,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_RemoveAll_1() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                });
                e.RemoveAll(ev => ev.EventType == MidiEventType.NoteOn);
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_RemoveAll_2() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                });
                e.RemoveAll(ev => false);
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_RemoveAll_3() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TimeSignatureEvent(),
                    new NoteOffEvent(),
                });
                e.RemoveAll(ev => ev is TimeSignatureEvent);
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_RemoveAll_4() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new SetTempoEvent(),
                    new TimeSignatureEvent(),
                    new NoteOffEvent(),
                });
                e.RemoveAll(ev => ev.EventType == MidiEventType.TimeSignature);
            },
            isInitialState: false,
            hasTempoMapEvents: true,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_Clear_1() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.Clear();
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        [Test]
        public void CheckTempoMapProperties_Clear_2() => CheckTempoMapProperties(
            changeCollection: e =>
            {
                e.AddRange(new MidiEvent[]
                {
                    new TextEvent("A"),
                    new SetTempoEvent(),
                    new TimeSignatureEvent(),
                });
                e.Clear();
            },
            isInitialState: false,
            hasTempoMapEvents: false,
            lastTempoMapEventIndex: -1);

        #endregion

        #region Private methods

        private void CheckTempoMapProperties(
            Action<EventsCollection> changeCollection,
            bool isInitialState,
            bool hasTempoMapEvents,
            int lastTempoMapEventIndex)
        {
            var eventsCollection = new EventsCollection();
            changeCollection(eventsCollection);

            Assert.AreEqual(isInitialState, eventsCollection.IsInitialState, $"{nameof(EventsCollection.IsInitialState)} is invalid.");
            Assert.AreEqual(hasTempoMapEvents, eventsCollection.HasTempoMapEvents, $"{nameof(EventsCollection.HasTempoMapEvents)} is invalid.");
            Assert.AreEqual(lastTempoMapEventIndex, eventsCollection.LastTempoMapEventIndex, $"{nameof(EventsCollection.LastTempoMapEventIndex)} is invalid.");
        }

        #endregion
    }
}
