namespace Melanchall.DryWetMidi.Core
{
    public sealed class MidiChunkEqualityCheckSettings
    {
        #region Properties

        public MidiEventEqualityCheckSettings EventEqualityCheckSettings { get; set; } = new MidiEventEqualityCheckSettings();

        #endregion
    }
}
