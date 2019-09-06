using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
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

        public Playback Playback { get; }

        public ITimeSpan Time { get; }

        #endregion
    }
}
