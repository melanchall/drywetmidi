using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Holds current times of playbacks for <see cref="PlaybackCurrentTimeWatcher.CurrentTimeChanged"/>.
    /// </summary>
    public sealed class PlaybackCurrentTimeChangedEventArgs : EventArgs
    {
        #region Constructor

        internal PlaybackCurrentTimeChangedEventArgs(IEnumerable<PlaybackCurrentTime> times)
        {
            Times = times;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets collection of current times of playbacks.
        /// </summary>
        public IEnumerable<PlaybackCurrentTime> Times { get; }

        #endregion
    }
}
