using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class ObservableTimedObjectsCollectionTests
    {
        #region Test methods

        [Test]
        public void Add_FromEmptyCollection_Single()
        {
            var newObject = new Note((SevenBitNumber)90, 100, 10);

            Add_FromEmptyCollection(
                new[] { newObject },
                collection => collection.Add(newObject));
        }

        [Test]
        public void Add_FromEmptyCollection_Multiple()
        {
            var newObject = new ITimedObject[]
            {
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            };

            Add_FromEmptyCollection(
                newObject,
                collection => collection.Add(newObject));
        }

        [Test]
        public void Add_FromEmptyCollection_Batch()
        {
            var newObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            };

            Add_FromEmptyCollection(
                newObjects,
                collection => collection.ChangeCollection(() => collection.Add(newObjects)));
        }

        [Test]
        public void Add_FromPreFilledCollection_Single_1()
        {
            var initialObjects = new[] { new TimedEvent(new TextEvent("B"), 20) };
            var newObjects = new Note((SevenBitNumber)90, 100, 10);

            Add_FromPreFilledCollection(
                initialObjects,
                new[] { newObjects },
                collection => collection.Add(newObjects));
        }

        [Test]
        public void Add_FromPreFilledCollection_Single_2()
        {
            var initialObjects = new[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new TimedEvent(new NoteOnEvent()),
                new TimedEvent(new NoteOffEvent(), 400),
            };
            var newObjects = new Note((SevenBitNumber)90, 100, 10);

            Add_FromPreFilledCollection(
                initialObjects,
                new[] { newObjects },
                collection => collection.Add(newObjects));
        }

        [Test]
        public void Add_FromPreFilledCollection_Multiple_1()
        {
            var initialObjects = new[] { new TimedEvent(new TextEvent("B"), 20) };
            var newObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            };

            Add_FromPreFilledCollection(
                initialObjects,
                newObjects,
                collection => collection.Add(newObjects));
        }

        [Test]
        public void Add_FromPreFilledCollection_Multiple_2()
        {
            var initialObjects = new[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new TimedEvent(new NoteOnEvent()),
                new TimedEvent(new NoteOffEvent(), 400),
            };
            var newObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            };

            Add_FromPreFilledCollection(
                initialObjects,
                newObjects,
                collection => collection.Add(newObjects));
        }

        [Test]
        public void Add_FromPreFilledCollection_Batch_1()
        {
            var initialObjects = new[] { new TimedEvent(new TextEvent("B"), 20) };
            var newObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            };

            Add_FromPreFilledCollection(
                initialObjects,
                newObjects,
                collection => collection.ChangeCollection(() => collection.Add(newObjects)));
        }

        [Test]
        public void Add_FromPreFilledCollection_Batch_2()
        {
            var initialObjects = new[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new TimedEvent(new NoteOnEvent()),
                new TimedEvent(new NoteOffEvent(), 400),
            };
            var newObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            };

            Add_FromPreFilledCollection(
                initialObjects,
                newObjects,
                collection => collection.ChangeCollection(() => collection.Add(newObjects)));
        }

        [Test]
        public void Remove_FromEmptyCollection_Single()
        {
            var oldObject = new TimedEvent(new TextEvent("B"), 20);

            Remove(
                Array.Empty<ITimedObject>(),
                new[] { oldObject },
                collection => collection.Remove(oldObject));
        }

        [Test]
        public void Remove_FromPreFilledCollection_Single_1()
        {
            var initialObjects = new[]
            {
                new TimedEvent(new TextEvent("B"), 20),
            };

            Remove(
                initialObjects,
                initialObjects,
                collection => collection.Remove(initialObjects.First()));
        }

        [Test]
        public void Remove_FromPreFilledCollection_Single_2()
        {
            var initialObjects = new[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new TimedEvent(new NoteOnEvent()),
                new TimedEvent(new NoteOffEvent(), 400),
            };
            var oldObject = initialObjects[DryWetMidi.Common.Random.Instance.Next(initialObjects.Length)];

            Remove(
                initialObjects,
                new[] { oldObject },
                collection => collection.Remove(oldObject));
        }

        [Test]
        public void Remove_FromPreFilledCollection_Multiple([Values(0, 1, 2, 3)] int count)
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            };
            var oldObjects = initialObjects.Take(count).ToArray();

            Remove(
                initialObjects,
                oldObjects,
                collection => collection.Remove(oldObjects));
        }

        [Test]
        public void Remove_FromPreFilledCollection_Batch([Values(0, 1, 2, 3)] int count)
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            };
            var oldObjects = initialObjects.Take(count).ToArray();

            Remove(
                initialObjects,
                oldObjects,
                collection => collection.ChangeCollection(() => collection.Remove(oldObjects)));
        }

        [Test]
        public void ChangeObject_Single()
        {
            var objectToChange = new TimedEvent(new TextEvent("A"), 5);
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new Note((SevenBitNumber)90, 100, 10),
                objectToChange,
            };

            ChangeObject(
                initialObjects,
                collection => collection.ChangeObject(objectToChange, obj => ((TextEvent)((TimedEvent)obj).Event).Text = "C"),
                new[] { objectToChange },
                new[] { new TimedEvent(new TextEvent("C"), 5) });
        }

        [Test]
        public void ChangeObject_Single_Time()
        {
            var objectToChange = new TimedEvent(new TextEvent("A"), 5);
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new Note((SevenBitNumber)90, 100, 10),
                objectToChange,
            };

            ChangeObject(
                initialObjects,
                collection => collection.ChangeObject(objectToChange, obj => obj.Time = 100),
                new[] { objectToChange },
                new[] { new TimedEvent(new TextEvent("A"), 100) });
        }

        [Test]
        public void ChangeObject_Single_Batch()
        {
            var objectToChange = new TimedEvent(new TextEvent("A"), 5);
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new Note((SevenBitNumber)90, 100, 10),
                objectToChange,
            };

            ChangeObject(
                initialObjects,
                collection => collection.ChangeCollection(() => collection.ChangeObject(objectToChange, obj => ((TextEvent)((TimedEvent)obj).Event).Text = "C")),
                new[] { objectToChange },
                new[] { new TimedEvent(new TextEvent("C"), 5) });
        }

        [Test]
        public void ChangeObject_Single_Time_Batch()
        {
            var objectToChange = new TimedEvent(new TextEvent("A"), 5);
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new Note((SevenBitNumber)90, 100, 10),
                objectToChange,
            };

            ChangeObject(
                initialObjects,
                collection => collection.ChangeCollection(() => collection.ChangeObject(objectToChange, obj => obj.Time = 100)),
                new[] { objectToChange },
                new[] { new TimedEvent(new TextEvent("A"), 100) });
        }

        [Test]
        public void ChangeCollection_General([Values(0, 1, 2, 3)] int deepLevel)
        {
            var initialObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            };

            var collection = new ObservableTimedObjectsCollection(initialObjects);

            var eventsArgs = new List<ObservableTimedObjectsCollectionChangedEventArgs>();
            collection.CollectionChanged += (_, e) => eventsArgs.Add(e);

            var objectToAdd1 = new TimedEvent(new TextEvent("C"), 15);
            var objectToAdd2 = new Note((SevenBitNumber)80, 200, 20);
            var objectsToAdd = new ITimedObject[]
            {
                objectToAdd1,
                objectToAdd2,
            };
            var objectToAdd3 = new Chord(
                new Note((SevenBitNumber)70, 20, 23),
                new Note((SevenBitNumber)60, 20, 24));

            Action action = () => collection.ChangeCollection(() =>
            {
                collection.Add(objectsToAdd);
                collection.Remove(new TimedEvent(new NoteOnEvent()));
                collection.Add(objectToAdd3);
                collection.Remove(objectToAdd2);
                collection.ChangeObject(
                    objectToAdd3,
                    obj =>
                    {
                        obj.Time = 100;
                        ((Chord)obj).Notes.First().Velocity = (SevenBitNumber)50;
                    });
            });

            for (var i = 0; i < deepLevel; i++)
            {
                var a = action;
                action = () => collection.ChangeCollection(a);
            }

            action();

            Assert.AreEqual(1, eventsArgs.Count, "Invalid events args count.");

            var eventArgs = eventsArgs.Single();

            CheckCollectionIsNullOrEmpty(eventArgs.RemovedObjects, "There are removed objects.");

            var addedObjects = eventArgs.AddedObjects;
            CollectionAssert.AreEquivalent(
                new ITimedObject[]
                {
                    objectToAdd1,
                    objectToAdd3,
                },
                addedObjects,
                "Invalid added objects.");

            var changedObjects = eventArgs.ChangedObjects;
            CollectionAssert.AreEqual(
                new ITimedObject[]
                {
                    objectToAdd3,
                },
                changedObjects.Select(o => o.TimedObject),
                "Invalid changed objects.");

            var changedObject = changedObjects.Single();
            MidiAsserts.AreEqual(
                new Chord(
                    new Note((SevenBitNumber)70, 20, 100) { Velocity = (SevenBitNumber)50 },
                    new Note((SevenBitNumber)60, 20, 101)),
                changedObject.TimedObject,
                "Invalid changed object.");
            Assert.AreEqual(23, changedObject.OldTime, "Invalid old time of changed object.");
        }

        [Test]
        public void ChangeCollection_AddAfterAdd()
        {
            var collection = new ObservableTimedObjectsCollection();

            var eventsArgs = new List<ObservableTimedObjectsCollectionChangedEventArgs>();
            collection.CollectionChanged += (_, e) => eventsArgs.Add(e);

            var objectToAdd1 = new TimedEvent(new TextEvent("C"), 15);
            var objectToAdd2 = new Note((SevenBitNumber)80, 200, 20);

            collection.ChangeCollection(() =>
            {
                collection.Add(objectToAdd1);
                collection.Add(objectToAdd2);
            });

            Assert.AreEqual(1, eventsArgs.Count, "Invalid events args count.");

            var eventArgs = eventsArgs.Single();
            CheckCollectionIsNullOrEmpty(eventArgs.RemovedObjects, "There are removed objects.");
            CheckCollectionIsNullOrEmpty(eventArgs.ChangedObjects, "There are changed objects.");

            var addedObjects = eventArgs.AddedObjects;
            CollectionAssert.AreEquivalent(
                new ITimedObject[]
                {
                    objectToAdd1,
                    objectToAdd2,
                },
                addedObjects,
                "Invalid added objects.");
        }

        [Test]
        public void ChangeCollection_AddAndRemoveSame()
        {
            var collection = new ObservableTimedObjectsCollection(new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            });

            var eventsArgs = new List<ObservableTimedObjectsCollectionChangedEventArgs>();
            collection.CollectionChanged += (_, e) => eventsArgs.Add(e);

            var objectToAdd = new TimedEvent(new TextEvent("C"), 15);

            collection.ChangeCollection(() =>
            {
                collection.Add(objectToAdd);
                collection.Remove(objectToAdd);
            });

            Assert.AreEqual(0, eventsArgs.Count, "Invalid events args count.");
        }

        [Test]
        public void ChangeCollection_AddAndRemoveAndAddSame()
        {
            var collection = new ObservableTimedObjectsCollection(new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            });

            var eventsArgs = new List<ObservableTimedObjectsCollectionChangedEventArgs>();
            collection.CollectionChanged += (_, e) => eventsArgs.Add(e);

            var objectToAdd = new TimedEvent(new TextEvent("C"), 15);

            collection.ChangeCollection(() =>
            {
                collection.Add(objectToAdd);
                collection.Remove(objectToAdd);
                collection.Add(objectToAdd);
            });

            Assert.AreEqual(1, eventsArgs.Count, "Invalid events args count.");

            var eventArgs = eventsArgs.Single();

            CheckCollectionIsNullOrEmpty(eventArgs.RemovedObjects, "There are removed objects.");
            CheckCollectionIsNullOrEmpty(eventArgs.ChangedObjects, "There are changed objects.");

            var addedObjects = eventArgs.AddedObjects;
            CollectionAssert.AreEquivalent(
                new ITimedObject[]
                {
                    objectToAdd,
                },
                addedObjects,
                "Invalid added objects.");
        }

        [Test]
        public void ChangeCollection_AddAndChangeAndRemoveSame()
        {
            var collection = new ObservableTimedObjectsCollection(new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
            });

            var eventsArgs = new List<ObservableTimedObjectsCollectionChangedEventArgs>();
            collection.CollectionChanged += (_, e) => eventsArgs.Add(e);

            var objectToAdd = new TimedEvent(new TextEvent("C"), 15);

            collection.ChangeCollection(() =>
            {
                collection.Add(objectToAdd);
                collection.ChangeObject(objectToAdd, obj => obj.Time = 20);
                collection.Remove(objectToAdd);
            });

            Assert.AreEqual(0, eventsArgs.Count, "Invalid events args count.");
        }

        [Test]
        public void ChangeCollection_AddAndRemoveSome()
        {
            var collection = new ObservableTimedObjectsCollection();

            var eventsArgs = new List<ObservableTimedObjectsCollectionChangedEventArgs>();
            collection.CollectionChanged += (_, e) => eventsArgs.Add(e);

            var objectToAdd1 = new TimedEvent(new TextEvent("C"), 15);
            var objectToAdd2 = new Note((SevenBitNumber)80, 200, 20);

            collection.ChangeCollection(() =>
            {
                collection.Add(objectToAdd1, objectToAdd2);
                collection.Remove(objectToAdd1);
            });

            Assert.AreEqual(1, eventsArgs.Count, "Invalid events args count.");

            var eventArgs = eventsArgs.Single();

            CheckCollectionIsNullOrEmpty(eventArgs.RemovedObjects, "There are removed objects.");
            CheckCollectionIsNullOrEmpty(eventArgs.ChangedObjects, "There are changed objects.");

            var addedObjects = eventArgs.AddedObjects;
            CollectionAssert.AreEquivalent(
                new ITimedObject[]
                {
                    objectToAdd2,
                },
                addedObjects,
                "Invalid added objects.");
        }

        [Test]
        public void ChangeCollection_ChangeAndChangeSame()
        {
            var objectToChange = new TimedEvent(new TextEvent("C"), 15);
            var collection = new ObservableTimedObjectsCollection(new[] { objectToChange });

            var eventsArgs = new List<ObservableTimedObjectsCollectionChangedEventArgs>();
            collection.CollectionChanged += (_, e) => eventsArgs.Add(e);

            collection.ChangeCollection(() =>
            {
                collection.ChangeObject(objectToChange, obj => obj.Time = 20);
                collection.ChangeObject(objectToChange, obj => obj.Time = 30);
                collection.ChangeObject(objectToChange, obj => ((TextEvent)((TimedEvent)obj).Event).Text = "ABC");
            });

            Assert.AreEqual(1, eventsArgs.Count, "Invalid events args count.");

            var eventArgs = eventsArgs.Single();

            CheckCollectionIsNullOrEmpty(eventArgs.RemovedObjects, "There are removed objects.");
            CheckCollectionIsNullOrEmpty(eventArgs.AddedObjects, "There are added objects.");

            var changedObjects = eventArgs.ChangedObjects;
            CollectionAssert.AreEqual(new[] { objectToChange }, changedObjects.Select(o => o.TimedObject), "Invalid changed objects.");

            var changedObject = eventArgs.ChangedObjects.Single();
            MidiAsserts.AreEqual(
                new TimedEvent(new TextEvent("ABC"), 30),
                changedObject.TimedObject,
                "Invalid changed object");
            Assert.AreEqual(15, changedObject.OldTime, "Invalid old time of changed object.");
        }

        [Test]
        public void ChangeCollection_RemoveAddChangeSame()
        {
            var objectToChange = new TimedEvent(new TextEvent("C"), 15);

            var collection = new ObservableTimedObjectsCollection(new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 20),
                new Note((SevenBitNumber)90, 100, 10),
                new TimedEvent(new TextEvent("A"), 5),
                objectToChange,
            });

            var eventsArgs = new List<ObservableTimedObjectsCollectionChangedEventArgs>();
            collection.CollectionChanged += (_, e) => eventsArgs.Add(e);

            collection.ChangeCollection(() =>
            {
                collection.Remove(objectToChange);
                collection.Add(objectToChange);
                collection.ChangeObject(objectToChange, obj => obj.Time = 20);
            });

            Assert.AreEqual(1, eventsArgs.Count, "Invalid events args count.");

            var eventArgs = eventsArgs.Single();

            CheckCollectionIsNullOrEmpty(eventArgs.RemovedObjects, "There are removed objects.");
            CheckCollectionIsNullOrEmpty(eventArgs.AddedObjects, "There are added objects.");

            var changedObjects = eventArgs.ChangedObjects;
            CollectionAssert.AreEqual(new[] { objectToChange }, changedObjects.Select(o => o.TimedObject), "Invalid changed objects.");

            var changedObject = eventArgs.ChangedObjects.Single();
            MidiAsserts.AreEqual(
                new TimedEvent(new TextEvent("C"), 20),
                changedObject.TimedObject,
                "Invalid changed object");
            Assert.AreEqual(15, changedObject.OldTime, "Invalid old time of changed object.");

        }

        #endregion

        #region Private methods

        private void ChangeObject(
            IEnumerable<ITimedObject> initialObjects,
            Action<ObservableTimedObjectsCollection> change,
            IEnumerable<ITimedObject> updatedObjects,
            IEnumerable<ITimedObject> updatedObjectsWithNewValues)
        {
            Dictionary<ITimedObject, long> oldTimes = default;
            ICollection<ITimedObject> oldObjects = default;

            CheckObservableTimedObjectsCollection_FromPreFilled(
                initialObjects,
                change,
                args =>
                {
                    Assert.AreEqual(1, args.Length, "Invalid events args count.");

                    var eventArgs = args.FirstOrDefault();
                    CheckCollectionIsNullOrEmpty(eventArgs?.AddedObjects, "There are added objects.");
                    CheckCollectionIsNullOrEmpty(eventArgs?.RemovedObjects, "There are removed objects.");

                    var changedObjects = eventArgs.ChangedObjects;
                    CollectionAssert.AreEqual(updatedObjects, changedObjects.Select(o => o.TimedObject), "Invalid changed objects.");
                    CollectionAssert.AreEqual(updatedObjects, oldTimes.Keys, "Invalid changed objects via old timed map.");
                    MidiAsserts.AreEqual(updatedObjectsWithNewValues, changedObjects.Select(obj => obj.TimedObject).ToArray(), "Invalid changed objects via new-values objects.");

                    var changedObjectsOldTimes = changedObjects.Select(obj => obj.OldTime).ToArray();
                    Assert.AreEqual(changedObjects.Count, changedObjectsOldTimes.Length, "Invalid changed objects old times count.");
                    Assert.AreEqual(updatedObjects.Count(), changedObjectsOldTimes.Length, "Invalid changed objects old times count.");

                    CollectionAssert.AreEquivalent(oldTimes.Values, changedObjectsOldTimes, "Invalid changed objects old times.");
                },
                collection => CollectionAssert.AreEquivalent(oldObjects, collection, "Invalid collection after change."),
                collection =>
                {
                    oldObjects = collection.ToArray();
                    oldTimes = collection
                        .Where(updatedObjects.Contains)
                        .ToDictionary(
                            obj => obj,
                            obj => obj.Time);
                });
        }

        private void Remove(
            IEnumerable<ITimedObject> initialObjects,
            IEnumerable<ITimedObject> oldObjects,
            Action<ObservableTimedObjectsCollection> remove) => CheckObservableTimedObjectsCollection_FromPreFilled(
                initialObjects,
                remove,
                args =>
                {
                    if (initialObjects.Any() && oldObjects.Any())
                        Assert.AreEqual(1, args.Length, "Invalid events args count.");

                    var eventArgs = args.FirstOrDefault();
                    CheckCollectionIsNullOrEmpty(eventArgs?.AddedObjects, "There are added objects.");
                    CheckCollectionIsNullOrEmpty(eventArgs?.ChangedObjects, "There are changed objects.");

                    if (initialObjects.Any() && oldObjects.Any())
                    {
                        var removedObjects = eventArgs.RemovedObjects;
                        CollectionAssert.AreEqual(oldObjects, removedObjects, "Invalid removed objects.");
                    }
                    else
                        CheckCollectionIsNullOrEmpty(eventArgs?.RemovedObjects, "There are removed objects.");
                },
                collection => CollectionAssert.AreEqual(initialObjects.Except(oldObjects).OrderBy(o => o.Time), collection, "Invalid collection after change."));

        private void Add_FromEmptyCollection(
            IEnumerable<ITimedObject> timedObjects,
            Action<ObservableTimedObjectsCollection> add) => CheckObservableTimedObjectsCollection_FromEmpty(
                add,
                args =>
                {
                    Assert.AreEqual(1, args.Length, "Invalid events args count.");

                    var eventArgs = args.First();
                    CheckCollectionIsNullOrEmpty(eventArgs.RemovedObjects, "There are removed objects.");
                    CheckCollectionIsNullOrEmpty(eventArgs.ChangedObjects, "There are changed objects.");

                    var addedObjects = eventArgs.AddedObjects;
                    CollectionAssert.AreEqual(timedObjects, addedObjects, "Invalid added objects.");
                },
                collection => CollectionAssert.AreEqual(timedObjects.OrderBy(o => o.Time), collection, "Invalid collection after change."));

        private void Add_FromPreFilledCollection(
            IEnumerable<ITimedObject> initialObjects,
            IEnumerable<ITimedObject> newObjects,
            Action<ObservableTimedObjectsCollection> add) => CheckObservableTimedObjectsCollection_FromPreFilled(
                initialObjects,
                add,
                args =>
                {
                    Assert.AreEqual(1, args.Length, "Invalid events args count.");

                    var eventArgs = args.First();
                    CheckCollectionIsNullOrEmpty(eventArgs.RemovedObjects, "There are removed objects.");
                    CheckCollectionIsNullOrEmpty(eventArgs.ChangedObjects, "There are changed objects.");

                    var addedObjects = eventArgs.AddedObjects;
                    CollectionAssert.AreEqual(newObjects, addedObjects, "Invalid added objects.");
                },
                collection => CollectionAssert.AreEqual(initialObjects.Concat(newObjects).OrderBy(o => o.Time), collection, "Invalid collection after change."));

        private void CheckObservableTimedObjectsCollection_FromEmpty(
            Action<ObservableTimedObjectsCollection> changeCollection,
            Action<ObservableTimedObjectsCollectionChangedEventArgs[]> checkEventsData,
            Action<ObservableTimedObjectsCollection> checkCollection) =>
            CheckObservableTimedObjectsCollection(
                () => new ObservableTimedObjectsCollection(),
                changeCollection,
                checkEventsData,
                checkCollection);

        private void CheckObservableTimedObjectsCollection_FromPreFilled(
            IEnumerable<ITimedObject> initialObjects,
            Action<ObservableTimedObjectsCollection> changeCollection,
            Action<ObservableTimedObjectsCollectionChangedEventArgs[]> checkEventsData,
            Action<ObservableTimedObjectsCollection> checkCollection,
            Action<ObservableTimedObjectsCollection> beforeChangeCollection = null) =>
            CheckObservableTimedObjectsCollection(
                () => new ObservableTimedObjectsCollection(initialObjects),
                changeCollection,
                checkEventsData,
                checkCollection,
                beforeChangeCollection);

        private void CheckObservableTimedObjectsCollection(
            Func<ObservableTimedObjectsCollection> createCollection,
            Action<ObservableTimedObjectsCollection> changeCollection,
            Action<ObservableTimedObjectsCollectionChangedEventArgs[]> checkEventsData,
            Action<ObservableTimedObjectsCollection> checkCollection,
            Action<ObservableTimedObjectsCollection> beforeChangeCollection = null)
        {
            var eventsArgs = new List<ObservableTimedObjectsCollectionChangedEventArgs>();

            var collection = createCollection();
            collection.CollectionChanged += (_, e) => eventsArgs.Add(e);

            beforeChangeCollection?.Invoke(collection);
            changeCollection(collection);

            checkEventsData(eventsArgs.ToArray());
            checkCollection(collection);
        }

        private void CheckCollectionIsNullOrEmpty<T>(ICollection<T> collection, string message) =>
            Assert.IsTrue(collection == null || collection.Count == 0, message);

        #endregion
    }
}
