namespace Melanchall.DryWetMidi.Devices
{
    public sealed class MidiClockSettings
    {
        #region Properties

        public CreateTickGeneratorCallback CreateTickGeneratorCallback { get; set; } =
            interval => new HighPrecisionTickGenerator(interval);

        #endregion
    }
}
