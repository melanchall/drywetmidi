namespace Melanchall.DryWetMidi.Tools
{
    public abstract class LengthedObjectsQuantizingSettings : QuantizingSettings
    {
        #region Properties

        public LengthedObjectTarget QuantizingTarget { get; set; }

        public QuantizingBeyondZeroPolicy QuantizingBeyondZeroPolicy { get; set; }

        public QuantizingBeyondFixedEndPolicy QuantizingBeyondFixedEndPolicy { get; set; }

        public bool FixOppositeEnd { get; set; }

        #endregion
    }
}
