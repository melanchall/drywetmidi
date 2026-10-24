using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedEventsManagingUtilitiesTests
    {
        #region Nested classes

        private class CustomTimedEvent : TimedEvent
        {
            public CustomTimedEvent(MidiEvent midiEvent, long time, int eventsCollectionIndex)
                : base(midiEvent, time)
            {
                EventsCollectionIndex = eventsCollectionIndex;
            }

            public int EventsCollectionIndex { get; }

            public override ITimedObject Clone() =>
                new CustomTimedEvent(Event, Time, EventsCollectionIndex);

            public override bool Equals(object obj) =>
                (obj as CustomTimedEvent).EventsCollectionIndex == EventsCollectionIndex;
        }

        private sealed class CustomTimedEvent2 : CustomTimedEvent
        {
            public CustomTimedEvent2(MidiEvent midiEvent, long time, int eventsCollectionIndex, int eventIndex)
                : base(midiEvent, time, eventsCollectionIndex)
            {
                EventIndex = eventIndex;
            }

            public int EventIndex { get; }

            public override ITimedObject Clone() =>
                new CustomTimedEvent2(Event, Time, EventsCollectionIndex, EventIndex);

            public override bool Equals(object obj) =>
                base.Equals(obj) &&
                (obj as CustomTimedEvent2).EventIndex == EventIndex;
        }

        #endregion

        #region Private methods

        private void CheckGetTimedEventsAsync(
            IEnumerable<TrackChunk> tracks,
            IEnumerable<ITimedObject> expectedTimedEvents,
            TimedEventDetectionSettings settings)
        {
            TrackChunk[] CreateTracks() => tracks.Select(track => (TrackChunk)track.Clone()).ToArray();

            var asyncTracks = CreateTracks();
            var timedEvents = asyncTracks.GetTimedEventsAsync(settings, cancellationToken: CancellationToken.None).Result;

            MidiAsserts.AreEqual(expectedTimedEvents, timedEvents, false, 0, "Async tracks timed events are invalid.");
            ClassicAssert.IsFalse(timedEvents.Any(ev => asyncTracks.SelectMany(track => track.Events).Any(e => ReferenceEquals(e, ev.Event))), "Async track detection returned source event references instead of clones.");

            var asyncFile = new MidiFile(CreateTracks());
            timedEvents = asyncFile.GetTimedEventsAsync(settings, cancellationToken: CancellationToken.None).Result;

            MidiAsserts.AreEqual(expectedTimedEvents, timedEvents, false, 0, "Async file timed events are invalid.");
            ClassicAssert.IsFalse(timedEvents.Any(ev => asyncFile.GetTrackChunks().SelectMany(track => track.Events).Any(e => ReferenceEquals(e, ev.Event))), "Async file detection returned source event references instead of clones.");

            var objects = CreateTracks().GetObjectsAsync(
                ObjectType.TimedEvent,
                new ObjectDetectionSettings { TimedEventDetectionSettings = settings },
                cancellationToken: CancellationToken.None).Result;

            MidiAsserts.AreEqual(expectedTimedEvents, objects, false, 0, "Async timed events from GetObjects are invalid.");

            timedEvents = InteractionAsyncTests.AsAsync(CreateTracks())
                .GetTimedEventsAsync(settings, cancellationToken: CancellationToken.None).Result;

            MidiAsserts.AreEqual(expectedTimedEvents, timedEvents, false, 0, "Timed events from an async track source are invalid.");
        }

        #endregion

        #region Constants

        private static readonly TimedEventDetectionSettings CustomEventSettings = new TimedEventDetectionSettings
        {
            Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventsCollectionIndex)
        };

        private static readonly TimedEventDetectionSettings CustomEventSettings2 = new TimedEventDetectionSettings
        {
            Constructor = data => new CustomTimedEvent2(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
        };

        #endregion
    }
}
