using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Holds information about changes within a collection for <see cref="IObservableTimedObjectsCollection.CollectionChanged"/> event.
    /// </summary>
    public sealed class ObservableTimedObjectsCollectionChangedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets or sets a collection of added objects.
        /// </summary>
        public ICollection<ITimedObject> AddedObjects { get; set; }

        /// <summary>
        /// Gets or sets a collection of removed objects.
        /// </summary>
        public ICollection<ITimedObject> RemovedObjects { get; set; }

        /// <summary>
        /// Gets or sets a collection of changed objects.
        /// </summary>
        public ICollection<ChangedTimedObject> ChangedObjects { get; set; }

        internal bool HasData
        {
            get
            {
                return
                    AddedObjects?.Any() == true ||
                    RemovedObjects?.Any() == true ||
                    ChangedObjects?.Any() == true;
            }
        }

        #endregion
    }
}
