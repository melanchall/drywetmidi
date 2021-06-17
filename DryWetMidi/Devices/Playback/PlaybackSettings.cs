namespace Melanchall.DryWetMidi.Devices
{
    public sealed class PlaybackSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets settings of the internal playback's clock.
        /// </summary>
        public MidiClockSettings ClockSettings { get; set; }

        #endregion
    }
}
