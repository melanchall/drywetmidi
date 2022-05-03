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
            using (var manager = new TimedObjectsManager<TObject>(eventsCollection, settings, new TimedObjectsComparerOnSameEventTime(sameTimeEventComparison)))
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
