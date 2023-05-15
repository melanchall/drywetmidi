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

        public override bool Equals(object obj)
        {
            var objectAt = obj as TimedObjectAt<TObject>;
            return objectAt != null && objectAt.AtIndex == AtIndex && objectAt.Object.Equals(Object);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + Object.GetHashCode();
                result = result * 23 + AtIndex.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
