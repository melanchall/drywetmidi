using Melanchall.DryWetMidi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ObservableTimedObjectsCollection : IObservableTimedObjectsCollection, IEnumerable<ITimedObject>
    {
        #region Events

        public event EventHandler<ObservableTimedObjectsCollectionChangedEventArgs> CollectionChanged;

        #endregion

        #region Fields

        private readonly List<ITimedObject> _objects = new List<ITimedObject>();
        private ICollection<ObservableTimedObjectsCollectionChangedEventArgs> _collectionChangedEventsArgs;

        #endregion

        #region Constructor

        public ObservableTimedObjectsCollection()
        {
        }

        public ObservableTimedObjectsCollection(IEnumerable<ITimedObject> timedObjects)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            Add(timedObjects);
        }

        #endregion

        #region Properties

        public int Count => _objects.Count;

        #endregion

        #region Methods

        public void ChangeCollection(Action change)
        {
            ThrowIfArgument.IsNull(nameof(change), change);

            var deepChange = _collectionChangedEventsArgs != null;
            _collectionChangedEventsArgs = _collectionChangedEventsArgs ?? new List<ObservableTimedObjectsCollectionChangedEventArgs>();

            try
            {
                change();

                if (!deepChange)
                    OnCollectionChanged(_collectionChangedEventsArgs);
            }
            finally
            {
                if (!deepChange)
                    _collectionChangedEventsArgs = null;
            }
        }

        public void ChangeObject(ITimedObject timedObject, Action<ITimedObject> change)
        {
            ThrowIfArgument.IsNull(nameof(timedObject), timedObject);
            ThrowIfArgument.IsNull(nameof(change), change);

            var oldTime = timedObject.Time;
            change(timedObject);
            HandleObjectChanged(timedObject, oldTime);
        }

        public void Add(IEnumerable<ITimedObject> timedObjects)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            var addedObjects = timedObjects.Where(o => o != null).ToList();
            _objects.AddRange(addedObjects);

            HandleObjectsAdded(addedObjects);
        }

        public void Add(params ITimedObject[] timedObjects)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            Add((IEnumerable<ITimedObject>)timedObjects);
        }

        public bool Remove(IEnumerable<ITimedObject> objects)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);

            var removedObjects = new List<ITimedObject>();
            
            foreach (var obj in objects)
            {
                if (obj != null && _objects.Remove(obj))
                    removedObjects.Add(obj);
            }

            HandleObjectsRemoved(removedObjects);

            return removedObjects.Any();
        }

        public bool Remove(params ITimedObject[] objects)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);

            return Remove((IEnumerable<ITimedObject>)objects);
        }

        // TODO: test
        public void Clear()
        {
            var removedObjects = _objects.ToList();
            _objects.Clear();

            HandleObjectsRemoved(removedObjects);
        }

        private void HandleObjectsAdded(IEnumerable<ITimedObject> addedObjects)
        {
            var eventsArgs = _collectionChangedEventsArgs ?? new List<ObservableTimedObjectsCollectionChangedEventArgs>();
            var eventArgs = new ObservableTimedObjectsCollectionChangedEventArgs();

            var eventArgsAddedObjects = eventArgs.AddedObjects;
            if (eventArgsAddedObjects == null)
                eventArgs.AddedObjects = eventArgsAddedObjects = new List<ITimedObject>();

            foreach (var obj in addedObjects)
            {
                eventArgsAddedObjects.Add(obj);
            }

            eventsArgs.Add(eventArgs);
            if (!IsInBatchOperation())
                OnCollectionChanged(eventsArgs);
        }

        private void HandleObjectsRemoved(IEnumerable<ITimedObject> removedObjects)
        {
            var eventsArgs = _collectionChangedEventsArgs ?? new List<ObservableTimedObjectsCollectionChangedEventArgs>();
            var eventArgs = new ObservableTimedObjectsCollectionChangedEventArgs();

            var eventArgsRemovedObjects = eventArgs.RemovedObjects;
            if (eventArgsRemovedObjects == null)
                eventArgs.RemovedObjects = eventArgsRemovedObjects = new List<ITimedObject>();

            foreach (var obj in removedObjects)
            {
                eventArgsRemovedObjects.Add(obj);
            }

            eventsArgs.Add(eventArgs);
            if (!IsInBatchOperation())
                OnCollectionChanged(eventsArgs);
        }

        private void HandleObjectChanged(ITimedObject timedObject, long oldTime)
        {
            var eventsArgs = _collectionChangedEventsArgs ?? new List<ObservableTimedObjectsCollectionChangedEventArgs>();
            var eventArgs = new ObservableTimedObjectsCollectionChangedEventArgs();

            var eventArgsChangedObjects = eventArgs.ChangedObjects;
            if (eventArgsChangedObjects == null)
                eventArgs.ChangedObjects = eventArgsChangedObjects = new List<ChangedTimedObject>();

            var changedTimedObject = new ChangedTimedObject(timedObject, oldTime);

            if (!eventArgsChangedObjects.Contains(changedTimedObject))
            {
                eventArgsChangedObjects.Add(changedTimedObject);
            }

            eventsArgs.Add(eventArgs);
            if (!IsInBatchOperation())
                OnCollectionChanged(eventsArgs);
        }

        private void OnCollectionChanged(ICollection<ObservableTimedObjectsCollectionChangedEventArgs> eventsArgs)
        {
            var allAddedObjects = new List<ITimedObject>();
            var allRemovedObjects = new List<ITimedObject>();
            var allChangedObjects = new List<ChangedTimedObject>();

            foreach (var eventArgs in eventsArgs)
            {
                var addedObjects = GetNonNullList(eventArgs.AddedObjects);
                var removedObjects = GetNonNullList(eventArgs.RemovedObjects);
                var changedObjects = GetNonNullList(eventArgs.ChangedObjects);

                allAddedObjects.AddRange(addedObjects);
                allRemovedObjects.AddRange(removedObjects);
                allChangedObjects.AddRange(changedObjects);
            }

            var changedObjectsHashSet = new HashSet<ITimedObject>();

            for (var i = 0; i < allChangedObjects.Count; i++)
            {
                var changedObject = allChangedObjects[i];
                if (!changedObjectsHashSet.Add(changedObject.TimedObject))
                {
                    allChangedObjects.RemoveAt(i);
                    i--;
                }
            }

            var finalAddedObjects = new List<ITimedObject>(allAddedObjects);
            var finalRemovedObjects = new List<ITimedObject>(allRemovedObjects);
            var finalChangedObjects = new List<ChangedTimedObject>(allChangedObjects);

            foreach (var obj in allRemovedObjects)
            {
                if (finalAddedObjects.Remove(obj))
                    finalRemovedObjects.Remove(obj);

                var removedChangedObjects = finalChangedObjects.Where(c => object.ReferenceEquals(c.TimedObject, obj)).ToArray();
                foreach (var removedChangedObject in removedChangedObjects)
                {
                    finalChangedObjects.Remove(removedChangedObject);
                }
            }

            foreach (var obj in allAddedObjects)
            {
                finalRemovedObjects.Remove(obj);
            }

            var args = new ObservableTimedObjectsCollectionChangedEventArgs
            {
                AddedObjects = finalAddedObjects,
                RemovedObjects = finalRemovedObjects,
                ChangedObjects = finalChangedObjects,
            };

            if (!args.HasData)
                return;

            CollectionChanged?.Invoke(this, args);
        }

        private List<T> GetNonNullList<T>(IEnumerable<T> source)
        {
            return (source ?? Enumerable.Empty<T>()).ToList();
        }

        private bool IsInBatchOperation()
        {
            return _collectionChangedEventsArgs != null;
        }

        #endregion

        #region IEnumerable<TObject>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ITimedObject> GetEnumerator()
        {
            return _objects.OrderBy(obj => obj.Time).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
