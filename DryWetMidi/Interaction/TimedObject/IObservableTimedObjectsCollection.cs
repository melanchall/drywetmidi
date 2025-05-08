using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents a collection which can be observed for changes via <see cref="CollectionChanged"/> event.
    /// </summary>
    public interface IObservableTimedObjectsCollection
    {
        #region Events

        /// <summary>
        /// Occurs when collection changed (objects have been added, removed or changed).
        /// </summary>
        event EventHandler<ObservableTimedObjectsCollectionChangedEventArgs> CollectionChanged;

        #endregion
    }
}
