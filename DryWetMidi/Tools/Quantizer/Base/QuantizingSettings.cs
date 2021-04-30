using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which quantizing should be performed.
    /// </summary>
    public abstract class QuantizingSettings<TObject>
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

        /// <summary>
        /// Gets or sets the level of quantizing from 0.0 (no quantizing) to 1.0 (full quantizng).
        /// </summary>
        /// <remarks>
        /// This setting specifies how close an object should be moved to nearest grid time. For example,
        /// 0.5 will lead to an object will be moved half the distance between its time and the nearest
        /// grid time.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is out of valid range.</exception>
        public double QuantizingLevel
        {
            get { return _quantizingLevel; }
            set
            {
                ThrowIfArgument.IsOutOfRange(nameof(value),
                                             value,
                                             NoQuantizingLevel,
                                             FullQuantizingLevel,
                                             $"Value is out of [{NoQuantizingLevel}; {FullQuantizingLevel}] range.");

                _quantizingLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets a predicate to filter objects that should be quantized. Use <c>null</c> if
        /// all objects should be processed.
        /// </summary>
        public Predicate<TObject> Filter { get; set; }

        #endregion
    }
}
