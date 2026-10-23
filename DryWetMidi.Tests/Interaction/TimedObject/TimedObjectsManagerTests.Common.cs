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
            Comparison<MidiEvent> sameTimeEventComparison = null,
            bool useAsync = false)
            where TObject : ITimedObject
        {
            if (!useAsync)
            {
                var asyncEvents = new EventsCollection();
                asyncEvents.AddRange(eventsCollection.Select(e => e.Clone()));

                CheckObjectsManager_Generic(
                    asyncEvents,
                    manageObjects,
                    expectedObjects,
                    expectedEvents,
                    settings,
                    sameTimeEventComparison,
                    useAsync: true);
            }

            var comparer = sameTimeEventComparison != null
                ? new TimedObjectsComparerOnSameEventTime(sameTimeEventComparison)
                : null;

            using (var manager = useAsync
                ? TimedObjectsManager<TObject>.CreateAsync(
                    eventsCollection,
                    settings,
                    comparer,
                    cancellationToken: System.Threading.CancellationToken.None).Result
                : new TimedObjectsManager<TObject>(eventsCollection, settings, comparer))
            {
                manageObjects(manager.Objects);
                MidiAsserts.AreEqual(
                    expectedObjects.OfType<ITimedObject>(),
                    manager.Objects.OfType<ITimedObject>(),
                    false,
                    0,
                    "Invalid objects on enumerating.");

                if (useAsync)
                {
                    manager.SaveChangesAsync(cancellationToken: System.Threading.CancellationToken.None).Wait();
                    MidiAsserts.AreEqual(
                        expectedEvents,
                        eventsCollection.GetTimedEvents(),
                        false,
                        0,
                        "Invalid events immediately after async save.");
                }
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
            Comparison<MidiEvent> sameTimeEventComparison = null,
            bool useAsync = false)
        {
            if (!useAsync)
            {
                var asyncEvents = new EventsCollection();
                asyncEvents.AddRange(eventsCollection.Select(e => e.Clone()));

                CheckObjectsManager(
                    asyncEvents,
                    objectType,
                    manageObjects,
                    expectedObjects,
                    expectedEvents,
                    settings,
                    sameTimeEventComparison,
                    useAsync: true);
            }

            var comparer = new TimedObjectsComparerOnSameEventTime(sameTimeEventComparison);

            using (var manager = useAsync
                ? TimedObjectsManager.CreateAsync(
                    eventsCollection,
                    objectType,
                    settings,
                    comparer,
                    cancellationToken: System.Threading.CancellationToken.None).Result
                : new TimedObjectsManager(eventsCollection, objectType, settings, comparer))
            {
                manageObjects(manager.Objects);
                MidiAsserts.AreEqual(
                    expectedObjects,
                    manager.Objects,
                    false,
                    0,
                    "Invalid objects on enumerating.");

                if (useAsync)
                {
                    manager.SaveChangesAsync(cancellationToken: System.Threading.CancellationToken.None).Wait();
                    MidiAsserts.AreEqual(
                        expectedEvents,
                        eventsCollection.GetTimedEvents(),
                        false,
                        0,
                        "Invalid events immediately after async save.");
                }
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
