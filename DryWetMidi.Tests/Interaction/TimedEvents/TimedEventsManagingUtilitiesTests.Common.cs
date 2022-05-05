using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

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
