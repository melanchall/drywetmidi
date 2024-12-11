namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ChangedTimedObject
    {
        #region Constructor

        public ChangedTimedObject(
            ITimedObject timedObject,
            long oldTime)
        {
            TimedObject = timedObject;
            OldTime = oldTime;
        }

        #endregion

        #region Properties

        public ITimedObject TimedObject { get; }

        public long OldTime { get; }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            var changedTimedObject = obj as ChangedTimedObject;
            if (changedTimedObject == null)
                return false;

            return
                object.ReferenceEquals(TimedObject, changedTimedObject.TimedObject) &&
                OldTime == changedTimedObject.OldTime;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + TimedObject.GetHashCode();
                result = result * 23 + OldTime.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            return $"{OldTime}: {TimedObject}";
        }

        #endregion
    }
}
