using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides data for the <see cref="TimedObjectsCollection{TObject}.CollectionChanged"/> event.
    /// </summary>
    public sealed class TimedObjectsCollectionChangedEventArgs<TObject> : EventArgs
        where TObject : ITimedObject
    {
        #region Constructor

        internal TimedObjectsCollectionChangedEventArgs(IEnumerable<TObject> addedObjects, IEnumerable<TObject> removedObjects)
        {
            AddedObjects = addedObjects;
            RemovedObjects = removedObjects;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets objects that were added to a <see cref="TimedObjectsCollection{TObject}"/>.
        /// </summary>
        public IEnumerable<TObject> AddedObjects { get; }

        /// <summary>
        /// Gets objects that were removed from a <see cref="TimedObjectsCollection{TObject}"/>.
        /// </summary>
        public IEnumerable<TObject> RemovedObjects { get; }

        #endregion
    }
}
