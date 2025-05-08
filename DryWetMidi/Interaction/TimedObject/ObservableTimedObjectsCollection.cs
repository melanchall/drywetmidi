using Melanchall.DryWetMidi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a collection which can be observed for changes via <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <seealso cref="IObservableTimedObjectsCollection"/>
    public sealed class ObservableTimedObjectsCollection : IObservableTimedObjectsCollection, IEnumerable<ITimedObject>
    {
        #region Constants

        private const short AddAction = 0;
        private const short RemoveAction = 1;
        private const short ChangeAction = 2;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when collection changed (objects have been added, removed or changed).
        /// </summary>
        public event EventHandler<ObservableTimedObjectsCollectionChangedEventArgs> CollectionChanged;

        #endregion

        #region Fields

        private readonly List<ITimedObject> _objects = new List<ITimedObject>();

        private bool _batchOperationInProgress = false;
        private readonly List<Tuple<short, ITimedObject, long>> _batchActions = new List<Tuple<short, ITimedObject, long>>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an empty instance of the <see cref="ObservableTimedObjectsCollection"/>.
        /// </summary>
        public ObservableTimedObjectsCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableTimedObjectsCollection"/> with
        /// the specified objects.
        /// </summary>
        /// <param name="timedObjects">Objects to add to the collection.</param>
        public ObservableTimedObjectsCollection(IEnumerable<ITimedObject> timedObjects)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            AddWithoutHandling(timedObjects);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of objects currently contained in the collection.
        /// </summary>
        public int Count => _objects.Count;

        #endregion

        #region Methods

        /// <summary>
        /// Executes a specified action that modifies the collection.
        /// </summary>
        /// <remarks>If the collection is already undergoing a batch operation, the <see cref="CollectionChanged"/>
        /// event will not be raised until the outermost operation completes. This ensures that multiple changes can be
        /// grouped together and processed as a single batch.</remarks>
        /// <param name="change">An <see cref="Action"/> that performs the modifications to the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="change"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Executes an action that modifies the specified object.
        /// </summary>
        /// <remarks>If the method is executed within the <see cref="ChangeCollection(Action)"/>,
        /// the <see cref="CollectionChanged"/> event will be fired when you're done with
        /// the <see cref="ChangeCollection(Action)"/> method.</remarks>
        /// <param name="timedObject">The object to be modified.</param>
        /// <param name="change">An <see cref="Action"/> that performs the modifications to the <paramref name="timedObject"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// One of the following errors occurred:
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timedObject"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="change"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void ChangeObject(ITimedObject timedObject, Action<ITimedObject> change)
        {
            ThrowIfArgument.IsNull(nameof(timedObject), timedObject);
            ThrowIfArgument.IsNull(nameof(change), change);

            var oldTime = timedObject.Time;
            change(timedObject);
            HandleObjectChanged(timedObject, oldTime);
        }

        /// <summary>
        /// Adds the specified objects to the current collection.
        /// </summary>
        /// <remarks>If the method is executed within the <see cref="ChangeCollection(Action)"/>,
        /// the <see cref="CollectionChanged"/> event will be fired when you're done with
        /// the <see cref="ChangeCollection(Action)"/> method.</remarks>
        /// <param name="objects">Objects to add to the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is <c>null</c>.</exception>
        public void Add(IEnumerable<ITimedObject> objects)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);

            var addedObjects = AddWithoutHandling(objects);

            HandleObjectsAdded(addedObjects);
        }

        /// <summary>
        /// Adds the specified objects to the current collection.
        /// </summary>
        /// <remarks>If the method is executed within the <see cref="ChangeCollection(Action)"/>,
        /// the <see cref="CollectionChanged"/> event will be fired when you're done with
        /// the <see cref="ChangeCollection(Action)"/> method.</remarks>
        /// <param name="objects">Objects to add to the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is <c>null</c>.</exception>
        public void Add(params ITimedObject[] objects)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);

            Add((IEnumerable<ITimedObject>)objects);
        }

        /// <summary>
        /// Removes the specified objects from the current collection.
        /// </summary>
        /// <remarks>If the method is executed within the <see cref="ChangeCollection(Action)"/>,
        /// the <see cref="CollectionChanged"/> event will be fired when you're done with
        /// the <see cref="ChangeCollection(Action)"/> method.</remarks>
        /// <param name="objects">Objects to remove from the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Removes the specified objects from the current collection.
        /// </summary>
        /// <remarks>If the method is executed within the <see cref="ChangeCollection(Action)"/>,
        /// the <see cref="CollectionChanged"/> event will be fired when you're done with
        /// the <see cref="ChangeCollection(Action)"/> method.</remarks>
        /// <param name="objects">Objects to remove from the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is <c>null</c>.</exception>
        public bool Remove(params ITimedObject[] objects)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);

            return Remove((IEnumerable<ITimedObject>)objects);
        }

        /// <summary>
        /// Removes all objects from the current collection.
        /// </summary>
        /// <remarks>If the method is executed within the <see cref="ChangeCollection(Action)"/>,
        /// the <see cref="CollectionChanged"/> event will be fired when you're done with
        /// the <see cref="ChangeCollection(Action)"/> method.</remarks>
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
                        changedObjects.RemoveWhere(obj => obj.Object == timedObject);
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
