using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class PlaybackTime
    {
        #region Constructor

        public PlaybackTime(TimeSpan time)
        {
            Time = time;
        }

        #endregion

        #region Properties

        public TimeSpan Time { get; set; }

        #endregion

        #region Operators

        public static implicit operator TimeSpan(PlaybackTime time)
        {
            return time.Time;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Time.ToString();
        }

        #endregion
    }
}
