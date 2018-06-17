using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which quantizing should be performed.
    /// </summary>
    public abstract class QuantizingSettings
    {
        #region Constants

        private const double NoQuantizingLevel = 0.0;
        private const double FullQuantizingLevel = 1.0;

        #endregion

        #region Fields

        private TimeSpanType _distanceCalculationType = TimeSpanType.Midi;
        private double _quantizingLevel = 1.0;

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

        public double QuantizingLevel
        {
            get { return _quantizingLevel; }
            set
            {
                ThrowIfArgument.IsOutOfRange(nameof(value),
                                             value,
                                             0.0,
                                             1.0,
                                             $"Value is out of [{NoQuantizingLevel}; {FullQuantizingLevel}] range.");

                _quantizingLevel = value;
            }
        }

        #endregion
    }
}
