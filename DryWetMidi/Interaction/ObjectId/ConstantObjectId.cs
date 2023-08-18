namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class ConstantObjectId<TId>
    {
        #region Constructor

        public ConstantObjectId(TId id)
        {
            Id = id;
        }

        #endregion

        #region Properties

        public TId Id { get; }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var objectId = obj as ConstantObjectId<TId>;
            if (ReferenceEquals(objectId, null))
                return false;

            return Id.Equals(objectId.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}
