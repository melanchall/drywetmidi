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
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Private methods

        private void ProcessObjects_EventsCollection_WithPredicate(
            ContainerType containerType,
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false,
            ObjectDetectionSettings settings = null)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            expectedProcessedCount,
                            eventsCollection.ProcessObjects(objectType, action, match, hint: hint, settings: settings),
                            "Invalid count of processed objects.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            newReferencesAllowed ||
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(eventsCollection);

                        Assert.AreEqual(
                            expectedProcessedCount,
                            trackChunk.ProcessObjects(objectType, action, match, hint: hint, settings: settings),
                            "Invalid count of processed objects.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            newReferencesAllowed ||
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        ProcessObjects_TrackChunks_WithPredicate(
                            objectType,
                            containerType == ContainerType.File,
                            new[] { midiEvents },
                            action,
                            match,
                            new[] { expectedMidiEvents },
                            expectedProcessedCount,
                            hint,
                            newReferencesAllowed,
                            settings);
                    }
                    break;
            }
        }

        private void ProcessObjects_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ObjectType objectType,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false,
            ObjectDetectionSettings settings = null)
        {
            var objectsCount = midiEvents.GetObjects(objectType, settings).Count;

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);

                        Assert.AreEqual(
                            objectsCount,
                            eventsCollection.ProcessObjects(objectType, action, hint: hint, settings: settings),
                            "Invalid count of processed objects.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            newReferencesAllowed ||
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(midiEvents);

                        Assert.AreEqual(
                            objectsCount,
                            trackChunk.ProcessObjects(objectType, action, hint: hint, settings: settings),
                            "Invalid count of processed objects.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            newReferencesAllowed ||
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        ProcessObjects_TrackChunks_WithoutPredicate(
                            objectType, 
                            containerType == ContainerType.File,
                            new[] { midiEvents },
                            action,
                            new[] { expectedMidiEvents },
                            hint,
                            newReferencesAllowed,
                            settings);
                    }
                    break;
            }
        }

        private void ProcessObjects_TrackChunks_WithPredicate(
            ObjectType objectType,
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false,
            ObjectDetectionSettings settings = null)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedProcessedCount,
                    midiFile.ProcessObjects(objectType, action, match, hint: hint, settings: settings),
                    "Invalid count of processed objects.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    newReferencesAllowed ||
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    expectedProcessedCount,
                    trackChunks.ProcessObjects(objectType, action, match, hint: hint, settings: settings),
                    "Invalid count of processed objects.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    newReferencesAllowed ||
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessObjects_TrackChunks_WithoutPredicate(
            ObjectType objectType,
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false,
            ObjectDetectionSettings settings = null)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var objectsCount = trackChunks.GetObjects(objectType, settings).Count;

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    objectsCount,
                    midiFile.ProcessObjects(objectType, action, hint: hint, settings: settings),
                    "Invalid count of processed objects.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    newReferencesAllowed ||
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    objectsCount,
                    trackChunks.ProcessObjects(objectType, action, hint: hint, settings: settings),
                    "Invalid count of processed objects.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    newReferencesAllowed ||
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessObjects_DetectionSettings_EventsCollection_WithPredicate(
            ContainerType containerType,
            ObjectType objectType,
            ObjectDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            expectedProcessedCount,
                            eventsCollection.ProcessObjects(objectType, action, match, settings),
                            "Invalid count of processed objects.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(eventsCollection);

                        Assert.AreEqual(
                            expectedProcessedCount,
                            trackChunk.ProcessObjects(objectType, action, match, settings),
                            "Invalid count of processed objects.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        ProcessObjects_DetectionSettings_TrackChunks_WithPredicate(
                            objectType,
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            action,
                            match,
                            new[] { expectedMidiEvents },
                            expectedProcessedCount);
                    }
                    break;
            }
        }

        private void ProcessObjects_DetectionSettings_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ObjectType objectType,
            ObjectDetectionSettings settings,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            var objectsCount = midiEvents.GetObjects(objectType, settings).Count();

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);

                        Assert.AreEqual(
                            objectsCount,
                            eventsCollection.ProcessObjects(objectType, action, settings),
                            "Invalid count of processed objects.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(midiEvents);

                        Assert.AreEqual(
                            objectsCount,
                            trackChunk.ProcessObjects(objectType, action, settings),
                            "Invalid count of processed objects.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        ProcessObjects_DetectionSettings_TrackChunks_WithoutPredicate(
                            objectType,
                            containerType == ContainerType.File,
                            settings,
                            new[] { midiEvents },
                            action,
                            new[] { expectedMidiEvents });
                    }
                    break;
            }
        }

        private void ProcessObjects_DetectionSettings_TrackChunks_WithPredicate(
            ObjectType objectType,
            bool wrapToFile,
            ObjectDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedProcessedCount,
                    midiFile.ProcessObjects(objectType, action, match, settings),
                    "Invalid count of processed objects.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    expectedProcessedCount,
                    trackChunks.ProcessObjects(objectType, action, match, settings),
                    "Invalid count of processed objects.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessObjects_DetectionSettings_TrackChunks_WithoutPredicate(
            ObjectType objectType,
            bool wrapToFile,
            ObjectDetectionSettings settings,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var objectsCount = trackChunks.GetObjects(objectType, settings).Count();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    objectsCount,
                    midiFile.ProcessObjects(objectType, action, settings),
                    "Invalid count of processed objects.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    objectsCount,
                    trackChunks.ProcessObjects(objectType, action, settings),
                    "Invalid count of processed objects.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
