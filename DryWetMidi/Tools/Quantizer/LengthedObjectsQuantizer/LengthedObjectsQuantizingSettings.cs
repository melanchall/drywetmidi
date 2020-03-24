using System.ComponentModel;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which lengthed objects should be quantized.
    /// </summary>
    public abstract class LengthedObjectsQuantizingSettings<TObject> : QuantizingSettings<TObject>
        where TObject : ILengthedObject
    {
        #region Fields

        private TimeSpanType _lengthType = TimeSpanType.Midi;
        private LengthedObjectTarget _quantizingTarget = LengthedObjectTarget.Start;
        private QuantizingBeyondZeroPolicy _quantizingBeyondZeroPolicy = QuantizingBeyondZeroPolicy.FixAtZero;
        private QuantizingBeyondFixedEndPolicy _quantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.CollapseAndFix;

        #endregion

        #region Properties

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
        /// The default value is <see cref="LengthedObjectTarget.Start"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public LengthedObjectTarget QuantizingTarget
        {
            get { return _quantizingTarget; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _quantizingTarget = value;
            }
        }

        /// <summary>
        /// Gets or sets policy according to which a quantizer should act in case of an object is going
        /// to be moved beyond zero. The default value is <see cref="Tools.QuantizingBeyondZeroPolicy.FixAtZero"/>.
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
        /// <see cref="Tools.QuantizingBeyondFixedEndPolicy.CollapseAndFix"/>.
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
        /// When an object's side is fixed the length can be changed during quantizing.
        /// </remarks>
        public bool FixOppositeEnd { get; set; }

        #endregion
    }
}
