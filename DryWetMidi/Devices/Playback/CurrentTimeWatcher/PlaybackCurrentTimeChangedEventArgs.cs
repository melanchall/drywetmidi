using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class PlaybackCurrentTimeChangedEventArgs : EventArgs
    {
        #region Constructor

        internal PlaybackCurrentTimeChangedEventArgs(IEnumerable<PlaybackCurrentTime> times)
        {
            Times = times;
        }

        #endregion

        #region Properties

        public IEnumerable<PlaybackCurrentTime> Times { get; }

        #endregion
    }
}
