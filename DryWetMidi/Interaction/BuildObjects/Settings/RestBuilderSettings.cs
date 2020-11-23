using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class RestBuilderSettings
    {
        #region Fields

        private RestSeparationPolicy _restSeparationPolicy = RestSeparationPolicy.NoSeparation;

        #endregion

        #region Properties

        public RestSeparationPolicy RestSeparationPolicy
        {
            get { return _restSeparationPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _restSeparationPolicy = value;
            }
        }

        #endregion
    }
}
