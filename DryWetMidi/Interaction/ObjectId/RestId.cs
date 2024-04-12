namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class RestId
    {
        #region Constructor

        public RestId(object key)
        {
            Key = key;
        }

        #endregion

        #region Properties

        public object Key { get; }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var restId = obj as RestId;
            if (ReferenceEquals(restId, null))
                return false;

            return Key?.Equals(restId.Key) == true;
        }

        public override int GetHashCode()
        {
            return Key?.GetHashCode() ?? 0;
        }

        #endregion
    }
}
