using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Settings which define how rests should be detected and built.
    /// </summary>
    public sealed class RestDetectionSettings
    {
        #region Fields

        private RestSeparationPolicy _restSeparationPolicy = RestSeparationPolicy.NoSeparation;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value that defines a rule for creating rests. The default value is
        /// <see cref="RestSeparationPolicy.NoSeparation"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
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
