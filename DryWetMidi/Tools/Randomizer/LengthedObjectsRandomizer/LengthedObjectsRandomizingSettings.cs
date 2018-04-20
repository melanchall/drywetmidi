namespace Melanchall.DryWetMidi.Tools
{
    public abstract class LengthedObjectsRandomizingSettings : RandomizingSettings
    {
        #region Properties

        public LengthedObjectTarget RandomizingTarget { get; set; }

        public bool FixOppositeEnd { get; set; }

        #endregion
    }
}
