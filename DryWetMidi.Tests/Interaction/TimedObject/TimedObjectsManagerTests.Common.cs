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
    public sealed partial class TimedObjectsManagerTests
    {
        #region Nested classes

        private sealed class TimedObjectsComparerOnSameEventTime : TimedObjectsComparer
        {
            private readonly Comparison<MidiEvent> _sameTimeEventsComparison;

            public TimedObjectsComparerOnSameEventTime()
                : this(null)
            {
            }

            public TimedObjectsComparerOnSameEventTime(Comparison<MidiEvent> sameTimeEventsComparison)
            {
                _sameTimeEventsComparison = sameTimeEventsComparison;
            }

            public override int Compare(ITimedObject x, ITimedObject y)
            {
                if (ReferenceEquals(x, y))
                    return 0;

                if (ReferenceEquals(x, null))
                    return -1;

                if (ReferenceEquals(y, null))
                    return 1;

                var baseResult = Math.Sign(x.Time - y.Time);
                if (baseResult != 0)
                    return baseResult;

                //

                var timedEventX = x as TimedEvent;
                var timedEventY = y as TimedEvent;
                if (timedEventX == null || timedEventY == null)
                    return 0;

                //

                return _sameTimeEventsComparison?.Invoke(timedEventX.Event, timedEventY.Event) ?? 0;
            }
        }

        #endregion

        #region Private methods

        private void CheckObjectsManager_Generic<TObject>(
            EventsCollection eventsCollection,
            Action<TimedObjectsCollection<TObject>> manageObjects,
            IEnumerable<TObject> expectedObjects,
            IEnumerable<TimedEvent> expectedEvents,
            ObjectDetectionSettings settings = null,
            Comparison<MidiEvent> sameTimeEventComparison = null)
            where TObject : ITimedObject
        {
            using (var manager = new TimedObjectsManager<TObject>(
                eventsCollection,
                settings,
                sameTimeEventComparison != null ? new TimedObjectsComparerOnSameEventTime(sameTimeEventComparison) : null))
            {
                manageObjects(manager.Objects);
                MidiAsserts.AreEqual(
                    expectedObjects.OfType<ITimedObject>(),
                    manager.Objects.OfType<ITimedObject>(),
                    false,
                    0,
                    "Invalid objects on enumerating.");
            }

            MidiAsserts.AreEqual(
                expectedEvents,
                eventsCollection.GetTimedEvents(),
                false,
                0,
                "Invalid events after source collection updated.");
        }

        private void CheckObjectsManager(
            EventsCollection eventsCollection,
            ObjectType objectType,
            Action<TimedObjectsCollection<ITimedObject>> manageObjects,
            IEnumerable<ITimedObject> expectedObjects,
            IEnumerable<TimedEvent> expectedEvents,
            ObjectDetectionSettings settings = null,
            Comparison<MidiEvent> sameTimeEventComparison = null)
        {
            using (var manager = new TimedObjectsManager(eventsCollection, objectType, settings, new TimedObjectsComparerOnSameEventTime(sameTimeEventComparison)))
            {
                manageObjects(manager.Objects);
                MidiAsserts.AreEqual(
                    expectedObjects,
                    manager.Objects,
                    false,
                    0,
                    "Invalid objects on enumerating.");
            }

            MidiAsserts.AreEqual(
                expectedEvents,
                eventsCollection.GetTimedEvents(),
                false,
                0,
                "Invalid events after source collection updated.");
        }

        #endregion
    }
}
