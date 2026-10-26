using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedEventsManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ManageTimedEvents_NoProcess_NoEvents() => ManageTimedEvents(
            midiEvents: Array.Empty<MidiEvent>(),
            settings: null,
            action: null,
            expectedEvents: Array.Empty<TimedEvent>(),
            expectedEventsInSource: Array.Empty<MidiEvent>());

        [Test]
        public void ManageTimedEvents_NoProcess_SingleEvent([Values(0, 10)] long deltaTime) => ManageTimedEvents(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = deltaTime }
            },
            settings: null,
            action: null,
            expectedEvents: new[]
            {
                new TimedEvent(new TextEvent("A"), deltaTime)
            },
            expectedEventsInSource: new[]
            {
                new TextEvent("A") { DeltaTime = deltaTime }
            });

        [Test]
        public void ManageTimedEvents_NoProcess_SingleEvent_Custom([Values(0, 10)] long deltaTime) => ManageTimedEvents(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = deltaTime }
            },
            settings: CustomEventSettings,
            action: null,
            expectedEvents: new[]
            {
                new CustomTimedEvent(new TextEvent("A"), deltaTime, 0)
            },
            expectedEventsInSource: new[]
            {
                new TextEvent("A") { DeltaTime = deltaTime }
            });

        #endregion

        #region Private methods

        private void ManageTimedEvents(
            ICollection<MidiEvent> midiEvents,
            TimedEventDetectionSettings settings,
            Action<ICollection<ITimedObject>> action,
            ICollection<TimedEvent> expectedEvents,
            ICollection<MidiEvent> expectedEventsInSource)
        {
            var trackChunk = new TrackChunk(midiEvents.Select(e => e.Clone()));

            using (var manager = trackChunk.Events.ManageTimedEvents(settings: settings))
            {
                MidiAsserts.AreEqual(expectedEvents, manager.Objects, false, 0, "Invalid timed events on events collection.");

                action?.Invoke(manager.Objects.Cast<ITimedObject>().ToArray());
            }

            MidiAsserts.AreEqual(expectedEventsInSource, trackChunk.Events, true, "Invalid events on events collection.");

            //

            trackChunk = new TrackChunk(midiEvents.Select(e => e.Clone()));

            using (var manager = trackChunk.ManageTimedEvents(settings: settings))
            {
                MidiAsserts.AreEqual(expectedEvents, manager.Objects, false, 0, "Invalid timed events on track chunk.");

                action?.Invoke(manager.Objects.Cast<ITimedObject>().ToArray());
            }

            MidiAsserts.AreEqual(new TrackChunk(expectedEventsInSource), trackChunk, true, "Invalid events on track chunk.");
        }

        #endregion
    }
}
