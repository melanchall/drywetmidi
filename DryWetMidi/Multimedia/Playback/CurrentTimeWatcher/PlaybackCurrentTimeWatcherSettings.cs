namespace Melanchall.DryWetMidi.Multimedia
{
    public sealed class PlaybackCurrentTimeWatcherSettings
    {
        #region Properties

        public MidiClockSettings ClockSettings { get; set; }

        public bool WatchOnlyRunningPlaybacks { get; set; }

        #endregion
    }
}
