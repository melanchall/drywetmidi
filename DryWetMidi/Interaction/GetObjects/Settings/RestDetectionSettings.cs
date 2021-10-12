using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Settings which define how rests should be detected and built.
    /// </summary>
    /// <remarks>
    /// Please see <see href="xref:a_getting_objects#rests">Getting objects
    /// (section GetObjects → Rests)</see> article to learn more.
    /// </remarks>
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
