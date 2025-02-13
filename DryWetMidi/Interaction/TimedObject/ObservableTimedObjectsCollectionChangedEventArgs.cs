using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ObservableTimedObjectsCollectionChangedEventArgs : EventArgs
    {
        #region Properties

        public ICollection<ITimedObject> AddedObjects { get; set; }

        public ICollection<ITimedObject> RemovedObjects { get; set; }

        public ICollection<ChangedTimedObject> ChangedObjects { get; set; }

        public bool HasData
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

        #region Overrides

        public override string ToString()
        {
            return $"+ {AddedObjects?.Count ?? 0} - {RemovedObjects?.Count ?? 0} ~ {ChangedObjects?.Count ?? 0}";
        }

        #endregion
    }
}
