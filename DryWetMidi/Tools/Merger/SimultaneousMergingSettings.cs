namespace Melanchall.DryWetMidi.Tools
{
    public sealed class SimultaneousMergingSettings
    {
        #region Properties

        public bool CopyNonTrackChunks { get; set; } = true;

        public bool IgnoreDifferentTempoMaps { get; set; }

        #endregion
    }
}
