using System;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    [Obsolete("OBS11")]
    public sealed class TimedEventsManagerTests
    {
        #region Test methods

        [Test]
        [Description("Check that TimedObjectsCollection is sorted when enumerated.")]
        public void ManageTimedEvents_EnumerationSorted()
        {
            using (var timedEventsManager = new TrackChunk().ManageTimedEvents())
            {
                var events = timedEventsManager.Objects;

                events.AddEvent(new NoteOnEvent(), 123);
                events.AddEvent(new NoteOnEvent(), 1);
                events.AddEvent(new NoteOnEvent(), 10);
                events.AddEvent(new NoteOnEvent(), 45);

                TimedObjectsCollectionTestUtilities.CheckTimedObjectsCollectionTimes(events, 1, 10, 45, 123);
            }
        }

        [Test]
        public void ManageTimedEvents_CustomSameTimeEventsComparer()
        {
            var originalTimedEvents = new[]
            {
                new TimedEvent(new LyricEvent("X"), 100),
                new TimedEvent(new LyricEvent("Y"), 100)
            };

            var expectedTimedEvents = new[]
            {
                new TimedEvent(new LyricEvent("Y"), 100),
                new TimedEvent(new LyricEvent("X"), 100)
            };

            //

            using (var timedEventsManager = new TrackChunk().ManageTimedEvents())
            {
                var events = timedEventsManager.Objects;
                events.Add(originalTimedEvents);

                var sortedEvents = events.ToList();

                MidiAsserts.AreEqual(originalTimedEvents, sortedEvents, false, 0, "Events have invalid order without custom same-time comparison.");
            }

            //

            Comparison<MidiEvent> sameTimeEventsComparison = (e1, e2) =>
            {
                var lyricEvent1 = (LyricEvent)e1;
                var lyricEvent2 = (LyricEvent)e2;

                if (lyricEvent1.Text == "X" && lyricEvent2.Text != "X")
                    return 1;

                return 0;
            };

            using (var timedEventsManager = new TrackChunk().ManageTimedEvents(comparer: new TimedObjectsComparerOnSameEventTime(sameTimeEventsComparison)))
            {
                var events = timedEventsManager.Objects;
                events.Add(originalTimedEvents);

                var sortedEvents = events.ToList();

                MidiAsserts.AreEqual(expectedTimedEvents, sortedEvents, false, 0, "Events have invalid order with custom same-time comparison.");
            }
        }

        #endregion
    }
}
