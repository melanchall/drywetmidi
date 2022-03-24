using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class TimedObjectsCollectionChangedEventArgs<TObject> : EventArgs
        where TObject : ITimedObject
    {
        #region Constructor

        public TimedObjectsCollectionChangedEventArgs(IEnumerable<TObject> addedObjects, IEnumerable<TObject> removedObjects)
        {
            AddedObjects = addedObjects;
            RemovedObjects = removedObjects;
        }

        #endregion

        #region Properties

        public IEnumerable<TObject> AddedObjects { get; }

        public IEnumerable<TObject> RemovedObjects { get; }

        #endregion
    }
}
