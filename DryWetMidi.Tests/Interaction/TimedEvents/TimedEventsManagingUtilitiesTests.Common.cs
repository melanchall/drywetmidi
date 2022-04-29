using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedEventsManagingUtilitiesTests
    {
        #region Nested classes

        private sealed class CustomTimedEvent : TimedEvent
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

        #endregion

        #region Constants

        private static readonly TimedEventDetectionSettings CustomEventSettings = new TimedEventDetectionSettings
        {
            Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventsCollectionIndex)
        };

        #endregion
    }
}
