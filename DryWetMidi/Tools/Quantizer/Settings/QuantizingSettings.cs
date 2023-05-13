using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which quantization should be performed by the <see cref="Quantizer"/>.
    /// More info in the <see href="xref:a_quantizer">Quantizer</see> article.
    /// </summary>
    public sealed class QuantizingSettings
    {
        #region Constants

        private const double NoQuantizingLevel = 0.0;
        private const double FullQuantizingLevel = 1.0;

        #endregion

        #region Fields

        private TimeSpanType _distanceCalculationType = TimeSpanType.Midi;
        private double _quantizingLevel = 1.0;
        private TimeSpanType _lengthType = TimeSpanType.Midi;
        private QuantizerTarget _quantizerTarget = QuantizerTarget.Start;
        private QuantizingBeyondZeroPolicy _quantizingBeyondZeroPolicy = QuantizingBeyondZeroPolicy.FixAtZero;
        private QuantizingBeyondFixedEndPolicy _quantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.CollapseAndFix;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets settings according to which randomizing should be performed.
        /// </summary>
        public RandomizingSettings RandomizingSettings { get; set; } = new RandomizingSettings();

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
        /// Gets or sets the level of quantization from 0.0 (no quantization) to 1.0 (full quantization).
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
                ThrowIfArgument.IsOutOfRange(
                    nameof(value),
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
        public Predicate<ITimedObject> Filter { get; set; }

        /// <summary>
        /// Gets or sets the type of an object's length that should be kept in case the opposite
        /// side is not fixed. The default value is <see cref="TimeSpanType.Midi"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public TimeSpanType LengthType
        {
            get { return _lengthType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _lengthType = value;
            }
        }

        /// <summary>
        /// Gets or sets the side of an object that should be quantized.
        /// The default value is <see cref="QuantizerTarget.Start"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public QuantizerTarget Target
        {
            get { return _quantizerTarget; }
            set { _quantizerTarget = value; }
        }

        /// <summary>
        /// Gets or sets policy according to which a quantizer should act in case of an object is going
        /// to be moved beyond zero. The default value is <see cref="QuantizingBeyondZeroPolicy.FixAtZero"/>.
        /// </summary>
        /// <remarks>
        /// When the start time of an object is not fixed, there is a chance that the object's end time
        /// will be quantized in a such way that the start time will be negative due to the object is
        /// moved to the left. Negative time is invalid so this policy provides options to prevent this
        /// situation.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public QuantizingBeyondZeroPolicy QuantizingBeyondZeroPolicy
        {
            get { return _quantizingBeyondZeroPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _quantizingBeyondZeroPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets policy according to which a quantizer should act in case of object's side
        /// is going to be moved beyond an opposite one that is fixed. The default value is
        /// <see cref="QuantizingBeyondFixedEndPolicy.CollapseAndFix"/>.
        /// </summary>
        /// <remarks>
        /// When one end of an object is fixed, there is a chance that the object's opposite end
        /// will be quantized in a such way that the object will be reversed resulting to negative length.
        /// This policy provides options to prevent this situation.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public QuantizingBeyondFixedEndPolicy QuantizingBeyondFixedEndPolicy
        {
            get { return _quantizingBeyondFixedEndPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _quantizingBeyondFixedEndPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether an opposite side of an object should be fixed or not.
        /// The default value is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// When an object's side is fixed the length can be changed during quantization.
        /// </remarks>
        public bool FixOppositeEnd { get; set; }

        #endregion
    }
}
