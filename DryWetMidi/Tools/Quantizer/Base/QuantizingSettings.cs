using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which quantizing should be performed.
    /// </summary>
    public abstract class QuantizingSettings
    {
        #region Fields

        private TimeSpanType _distanceCalculationType = TimeSpanType.Midi;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the type of distance calculation used to find a time to snap an object to.
        /// The default value is <see cref="TimeSpanType.Midi"/>.
        /// </summary>
        public TimeSpanType DistanceCalculationType
        {
            get { return _distanceCalculationType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _distanceCalculationType = value;
            }
        }

        #endregion
    }
}
