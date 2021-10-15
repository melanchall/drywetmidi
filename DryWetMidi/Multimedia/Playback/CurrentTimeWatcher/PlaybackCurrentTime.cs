using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Holds the current time of a playback.
    /// </summary>
    public sealed class PlaybackCurrentTime
    {
        #region Constructor

        internal PlaybackCurrentTime(Playback playback, ITimeSpan time)
        {
            Playback = playback;
            Time = time;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the playback which current <see cref="PlaybackCurrentTime"/> holds
        /// current time for.
        /// </summary>
        public Playback Playback { get; }

        /// <summary>
        /// Gets the current time of a playback.
        /// </summary>
        public ITimeSpan Time { get; }

        #endregion
    }
}
