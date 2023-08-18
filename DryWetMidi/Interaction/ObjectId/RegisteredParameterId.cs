namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class RegisteredParameterId
    {
        #region Constructor

        public RegisteredParameterId(RegisteredParameterType parameterType)
        {
            ParameterType = parameterType;
        }

        #endregion

        #region Properties

        public RegisteredParameterType ParameterType { get; }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var parameterId = obj as RegisteredParameterId;
            if (ReferenceEquals(parameterId, null))
                return false;

            return ParameterType == parameterId.ParameterType;
        }

        public override int GetHashCode()
        {
            return (int)ParameterType;
        }

        #endregion
    }
}
