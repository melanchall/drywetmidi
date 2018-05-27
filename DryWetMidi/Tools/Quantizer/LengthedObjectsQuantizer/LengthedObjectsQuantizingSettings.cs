using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public abstract class LengthedObjectsQuantizingSettings : QuantizingSettings
    {
        #region Fields

        private TimeSpanType _lengthType = TimeSpanType.Midi;

        private LengthedObjectTarget _quantizingTarget = LengthedObjectTarget.Start;

        private QuantizingBeyondZeroPolicy _quantizingBeyondZeroPolicy = QuantizingBeyondZeroPolicy.FixAtZero;

        private QuantizingBeyondFixedEndPolicy _quantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.CollapseAndFix;

        #endregion

        #region Properties

        public TimeSpanType LengthType
        {
            get { return _lengthType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _lengthType = value;
            }
        }

        public LengthedObjectTarget QuantizingTarget
        {
            get { return _quantizingTarget; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _quantizingTarget = value;
            }
        }

        public QuantizingBeyondZeroPolicy QuantizingBeyondZeroPolicy
        {
            get { return _quantizingBeyondZeroPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _quantizingBeyondZeroPolicy = value;
            }
        }

        public QuantizingBeyondFixedEndPolicy QuantizingBeyondFixedEndPolicy
        {
            get { return _quantizingBeyondFixedEndPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _quantizingBeyondFixedEndPolicy = value;
            }
        }

        public bool FixOppositeEnd { get; set; }

        #endregion
    }
}
