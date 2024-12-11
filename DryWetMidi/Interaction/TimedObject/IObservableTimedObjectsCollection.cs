using System;

namespace Melanchall.DryWetMidi.Interaction
{
    public interface IObservableTimedObjectsCollection
    {
        #region Events

        event EventHandler<ObservableTimedObjectsCollectionChangedEventArgs> CollectionChanged;

        #endregion
    }
}
