namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Settings for <see cref="PlaybackCurrentTimeWatcher"/>.
    /// </summary>
    public sealed class PlaybackCurrentTimeWatcherSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets settings for internal <see cref="MidiClock"/> used to watch
        /// playbacks by <see cref="PlaybackCurrentTimeWatcher"/>.
        /// </summary>
        public MidiClockSettings ClockSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether current time should be reported for only
        /// running playbacks (<see cref="Playback.IsRunning"/> is <c>true</c>) or not. The
        /// default value is <c>false</c> which means all playbacks will be watched.
        /// </summary>
        public bool WatchOnlyRunningPlaybacks { get; set; }

        #endregion
    }
}
