namespace Melanchall.DryWetMidi.Interaction
{
    public delegate void TimedObjectsCollectionChangedEventHandler<TObject>(
        TimedObjectsCollection<TObject> collection,
        TimedObjectsCollectionChangedEventArgs<TObject> args)
        where TObject : ITimedObject;
}
