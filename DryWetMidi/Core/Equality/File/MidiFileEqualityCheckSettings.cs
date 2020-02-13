namespace Melanchall.DryWetMidi.Core
{
    public sealed class MidiFileEqualityCheckSettings
    {
        #region Properties

        public bool CompareOriginalFormat { get; set; } = true;

        public MidiChunkEqualityCheckSettings ChunkEqualityCheckSettings { get; set; }

        #endregion
    }
}
