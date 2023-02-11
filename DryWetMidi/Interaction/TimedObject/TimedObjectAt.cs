namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class TimedObjectAt<TObject>
        where TObject : ITimedObject
    {
        #region Constructor

        public TimedObjectAt(TObject obj, int atIndex)
        {
            Object = obj;
            AtIndex = atIndex;
        }

        #endregion

        #region Properties

        public TObject Object { get; }

        public int AtIndex { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{Object} at {AtIndex}";
        }

        #endregion
    }
}
