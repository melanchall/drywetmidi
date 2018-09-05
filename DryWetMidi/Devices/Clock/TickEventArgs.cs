using System;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class TickEventArgs : EventArgs
    {
        #region Constructor

        public TickEventArgs(TimeSpan time)
        {
            Time = time;
        }

        #endregion

        #region Properties

        public TimeSpan Time { get; }

        #endregion
    }
}
