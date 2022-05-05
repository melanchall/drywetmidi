namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents the method that will handle the <see cref="TimedObjectsCollection{TObject}.CollectionChanged"/> event.
    /// </summary>
    /// <typeparam name="TObject">The type of objects within <see cref="TimedObjectsCollection{TObject}.CollectionChanged"/>.</typeparam>
    /// <param name="collection"><see cref="TimedObjectsCollection{TObject}"/> that has fired the event.</param>
    /// <param name="args">A <see cref="TimedObjectsCollectionChangedEventArgs{TObject}"/> that contains the event data.</param>
    public delegate void TimedObjectsCollectionChangedEventHandler<TObject>(
        TimedObjectsCollection<TObject> collection,
        TimedObjectsCollectionChangedEventArgs<TObject> args)
        where TObject : ITimedObject;
}
