namespace Melanchall.DryWetMidi.Interaction
{
    internal interface ISequentialObjectsBuilder
    {
        #region Methods

        bool TryAddObject(ITimedObject timedObject);

        #endregion
    }
}
