using System;
using System.Collections;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class GetTimedEventsAndNotesUtilitiesTests
    {
        #region Nested classes

        private sealed class TimedObjectComparer : IComparer
        {
            #region IComparer

            public int Compare(object x, object y)
            {
                var timedObject1 = x as ITimedObject;
                var timedObject2 = y as ITimedObject;

                if (ReferenceEquals(timedObject1, timedObject2))
                    return 1;

                if (ReferenceEquals(timedObject1, null))
                    return -1;

                if (ReferenceEquals(timedObject2, null))
                    return 1;

                var timesDifference = timedObject1.Time - timedObject2.Time;
                if (timesDifference != 0)
                    return Math.Sign(timesDifference);

                return TimedObjectEquality.AreEqual(timedObject1, timedObject2, false) ? 0 : -1;
            }

            #endregion
        }

        #endregion

        #region Test methods

        [OneTimeSetUp]
        public void SetUp()
        {
            TestContext.AddFormatter<ITimedObject>(obj =>
            {
                var timedObject = (ITimedObject)obj;
                var lengthedObject = obj as ILengthedObject;
                return lengthedObject != null
                    ? $"{obj} (T = {lengthedObject.Time}, L = {lengthedObject.Length})"
                    : $"{obj} (T = {timedObject.Time})";
            });
        }

        [Test]
        [Description("Make notes from empty collection of timed events.")]
        public void GetTimedEventsAndNotes_EmptyCollections()
        {
            var timedEvents = Enumerable.Empty<TimedEvent>();
            var actualObjects = timedEvents.GetTimedEventsAndNotes().ToList();
            var expectedObjects = Enumerable.Empty<TimedEvent>();

            CollectionAssert.AreEqual(expectedObjects, actualObjects, new TimedObjectComparer());
        }

        [Test]
        [Description("Make notes where they are all completed.")]
        public void GetTimedEventsAndNotes_AllProcessed()
        {
            var events = new MidiEvent[]
            {
                new SetTempoEvent(1234),
                new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { DeltaTime = 10, Channel = (FourBitNumber)1 },
                new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)70) { DeltaTime = 10, Channel = (FourBitNumber)1 },
                new PitchBendEvent(123) { DeltaTime = 10 },
                new MarkerEvent("Marker") { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 },
                new MarkerEvent("Marker 2") { DeltaTime = 10 },
                new TextEvent("Text") { DeltaTime = 10 },
                new TextEvent("Text 2") { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)1) { Channel = (FourBitNumber)10 },
                new CuePointEvent("Point") { DeltaTime = 10 },
                new NoteOffEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 },
                new NoteOffEvent((SevenBitNumber)1, (SevenBitNumber)0) { DeltaTime = 10, Channel = (FourBitNumber)1 },
                new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { Channel = (FourBitNumber)10 },
                new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { DeltaTime = 10, Channel = (FourBitNumber)1 }
            };

            var timedEvents = new TrackChunk(events).GetTimedEvents();
            var actualObjects = timedEvents.GetTimedEventsAndNotes().ToList();

            var expectedObjects = new ITimedObject[]
            {
                new TimedEvent(new SetTempoEvent(1234), 0),
                new Note((SevenBitNumber)1, 80, 10) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)100 },
                new Note((SevenBitNumber)2, 80, 20) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)70 },
                new TimedEvent(new PitchBendEvent(123), 30),
                new TimedEvent(new MarkerEvent("Marker"), 40),
                new Note((SevenBitNumber)3, 40, 40) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)1, OffVelocity = (SevenBitNumber)1 },
                new TimedEvent(new MarkerEvent("Marker 2"), 50),
                new TimedEvent(new TextEvent("Text"), 60),
                new TimedEvent(new TextEvent("Text 2"), 70),
                new Note((SevenBitNumber)2, 20, 70) { Channel = (FourBitNumber)10, Velocity = (SevenBitNumber)1 },
                new TimedEvent(new CuePointEvent("Point"), 80)
            };

            CollectionAssert.AreEqual(expectedObjects, actualObjects, new TimedObjectComparer());
        }

        [Test]
        [Description("Make notes where some notes aren't completed.")]
        public void GetTimedEventsAndNotes_NotAllProcessed()
        {
            var events = new MidiEvent[]
            {
                new SetTempoEvent(1234),
                new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { DeltaTime = 10, Channel = (FourBitNumber)1 },
                new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)70) { DeltaTime = 10, Channel = (FourBitNumber)1 },
                new PitchBendEvent(123) { DeltaTime = 10 },
                new MarkerEvent("Marker") { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 },
                new MarkerEvent("Marker 2") { DeltaTime = 10 },
                new TextEvent("Text") { DeltaTime = 10 },
                new TextEvent("Text 2") { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)1) { Channel = (FourBitNumber)10 },
                new CuePointEvent("Point") { DeltaTime = 10 },
                new NoteOffEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 },
                new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { Channel = (FourBitNumber)10 },
                new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { DeltaTime = 10, Channel = (FourBitNumber)1 },
                new NoteOffEvent((SevenBitNumber)78, (SevenBitNumber)0) { DeltaTime = 10, Channel = (FourBitNumber)11 }
            };

            var timedEvents = new TrackChunk(events).GetTimedEvents();
            var actualObjects = timedEvents.GetTimedEventsAndNotes().ToList();

            var expectedObjects = new ITimedObject[]
            {
                new TimedEvent(new SetTempoEvent(1234), 0),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { Channel = (FourBitNumber)1 }, 10),
                new Note((SevenBitNumber)2, 70, 20) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)70 },
                new TimedEvent(new PitchBendEvent(123), 30),
                new TimedEvent(new MarkerEvent("Marker"), 40),
                new Note((SevenBitNumber)3, 40, 40) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)1, OffVelocity = (SevenBitNumber)1 },
                new TimedEvent(new MarkerEvent("Marker 2"), 50),
                new TimedEvent(new TextEvent("Text"), 60),
                new TimedEvent(new TextEvent("Text 2"), 70),
                new Note((SevenBitNumber)2, 10, 70) { Channel = (FourBitNumber)10, Velocity = (SevenBitNumber)1 },
                new TimedEvent(new CuePointEvent("Point"), 80),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)78, (SevenBitNumber)0) { Channel = (FourBitNumber)11 }, 100)
            };

            CollectionAssert.AreEqual(expectedObjects, actualObjects, new TimedObjectComparer());
        }

        [Test]
        [Description("Make notes where there are same notes in tail of previous object.")]
        public void GetTimedEventsAndNotes_SameNotesInTail()
        {
            var events = new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { DeltaTime = 10 },
                new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)70) { DeltaTime = 10 },
                new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)1),
                new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)0),
                new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { DeltaTime = 10 },
                new NoteOffEvent((SevenBitNumber)1, (SevenBitNumber)0) { DeltaTime = 10 }
            };

            var timedEvents = new TrackChunk(events).GetTimedEvents().Concat(new[] { default(TimedEvent) });
            var actualObjects = timedEvents.GetTimedEventsAndNotes().ToList();

            var expectedObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)1, 30, 10) { Velocity = (SevenBitNumber)100 },
                new Note((SevenBitNumber)2, 0, 20) { Velocity = (SevenBitNumber)70, OffVelocity = (SevenBitNumber)1 },
                new Note((SevenBitNumber)2, 10, 20) { Velocity = (SevenBitNumber)0 },
                null
            };

            CollectionAssert.AreEqual(expectedObjects, actualObjects, new TimedObjectComparer());
        }

        #endregion
    }
}
