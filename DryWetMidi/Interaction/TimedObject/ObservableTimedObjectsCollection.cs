using Melanchall.DryWetMidi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ObservableTimedObjectsCollection : IObservableTimedObjectsCollection, IEnumerable<ITimedObject>
    {
        #region Constants

        private const short AddAction = 0;
        private const short RemoveAction = 1;
        private const short ChangeAction = 2;

        #endregion

        #region Events

        public event EventHandler<ObservableTimedObjectsCollectionChangedEventArgs> CollectionChanged;

        #endregion

        #region Fields

        private readonly List<ITimedObject> _objects = new List<ITimedObject>();

        private bool _batchOperationInProgress = false;
        private readonly List<Tuple<short, ITimedObject, long>> _batchActions = new List<Tuple<short, ITimedObject, long>>();

        #endregion

        #region Constructor

        public ObservableTimedObjectsCollection()
        {
        }

        public ObservableTimedObjectsCollection(IEnumerable<ITimedObject> timedObjects)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            AddWithoutHandling(timedObjects);
        }

        #endregion

        #region Properties

        public int Count => _objects.Count;

        #endregion

        #region Methods

        public void ChangeCollection(Action change)
        {
            ThrowIfArgument.IsNull(nameof(change), change);

            var deepChange = _batchOperationInProgress;
            _batchOperationInProgress = true;

            try
            {
                change();

                if (!deepChange)
                    OnCollectionChanged();
            }
            finally
            {
                if (!deepChange)
                {
                    _batchOperationInProgress = false;
                    _batchActions.Clear();
                }
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

            var addedObjects = AddWithoutHandling(timedObjects);

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

        public void Clear()
        {
            var removedObjects = _objects.ToList();
            _objects.Clear();

            HandleObjectsRemoved(removedObjects);
        }

        private ICollection<ITimedObject> AddWithoutHandling(IEnumerable<ITimedObject> timedObjects)
        {
            var addedObjects = timedObjects.Where(o => o != null).ToList();
            _objects.AddRange(addedObjects);

            return addedObjects;
        }

        private void HandleObjectsAdded(IEnumerable<ITimedObject> addedObjects)
        {
            if (_batchOperationInProgress)
            {
                foreach (var obj in addedObjects)
                {
                    _batchActions.Add(Tuple.Create(AddAction, obj, 0L));
                }

                return;
            }

            var eventArgs = new ObservableTimedObjectsCollectionChangedEventArgs();

            var eventArgsAddedObjects = eventArgs.AddedObjects;
            if (eventArgsAddedObjects == null)
                eventArgs.AddedObjects = eventArgsAddedObjects = new List<ITimedObject>();

            foreach (var obj in addedObjects)
            {
                eventArgsAddedObjects.Add(obj);
            }

            if (!eventArgs.HasData)
                return;

            CollectionChanged?.Invoke(this, eventArgs);
        }

        private void HandleObjectsRemoved(IEnumerable<ITimedObject> removedObjects)
        {
            if (_batchOperationInProgress)
            {
                foreach (var obj in removedObjects)
                {
                    _batchActions.Add(Tuple.Create(RemoveAction, obj, 0L));
                }

                return;
            }

            var eventArgs = new ObservableTimedObjectsCollectionChangedEventArgs();

            var eventArgsRemovedObjects = eventArgs.RemovedObjects;
            if (eventArgsRemovedObjects == null)
                eventArgs.RemovedObjects = eventArgsRemovedObjects = new List<ITimedObject>();

            foreach (var obj in removedObjects)
            {
                eventArgsRemovedObjects.Add(obj);
            }

            if (!eventArgs.HasData)
                return;

            CollectionChanged?.Invoke(this, eventArgs);
        }

        private void HandleObjectChanged(ITimedObject timedObject, long oldTime)
        {
            if (_batchOperationInProgress)
            {
                _batchActions.Add(Tuple.Create(ChangeAction, timedObject, oldTime));
                return;
            }

            var eventArgs = new ObservableTimedObjectsCollectionChangedEventArgs();

            var eventArgsChangedObjects = eventArgs.ChangedObjects;
            if (eventArgsChangedObjects == null)
                eventArgs.ChangedObjects = eventArgsChangedObjects = new List<ChangedTimedObject>();

            var changedTimedObject = new ChangedTimedObject(timedObject, oldTime);

            eventArgsChangedObjects.Add(changedTimedObject);

            if (!eventArgs.HasData)
                return;

            CollectionChanged?.Invoke(this, eventArgs);
        }

        private void OnCollectionChanged()
        {
            var addedObjects = new HashSet<ITimedObject>();
            var removedObjects = new HashSet<ITimedObject>();
            var changedObjects = new HashSet<ChangedTimedObject>();

            foreach (var item in _batchActions)
            {
                var action = item.Item1;
                var timedObject = item.Item2;
                var oldTime = item.Item3;

                switch (action)
                {
                    case AddAction:
                        addedObjects.Add(timedObject);
                        if (removedObjects.Remove(timedObject))
                            addedObjects.Remove(timedObject);
                        break;
                    case RemoveAction:
                        removedObjects.Add(timedObject);
                        changedObjects.RemoveWhere(obj => obj.TimedObject == timedObject);
                        if (addedObjects.Remove(timedObject))
                            removedObjects.Remove(timedObject);
                        break;
                    case ChangeAction:
                        changedObjects.Add(new ChangedTimedObject(timedObject, oldTime));
                        break;
                }
            }

            var args = new ObservableTimedObjectsCollectionChangedEventArgs
            {
                AddedObjects = addedObjects,
                RemovedObjects = removedObjects,
                ChangedObjects = changedObjects,
            };

            if (!args.HasData)
                return;

            CollectionChanged?.Invoke(this, args);
        }

        private List<T> GetNonNullList<T>(IEnumerable<T> source)
        {
            return (source ?? Enumerable.Empty<T>()).ToList();
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
