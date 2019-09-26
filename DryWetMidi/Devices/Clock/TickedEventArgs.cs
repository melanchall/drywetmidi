using System;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class TickedEventArgs : EventArgs
    {
        #region Constructor

        public TickedEventArgs(TimeSpan time)
        {
            Time = time;
        }

        #endregion

        #region Properties

        public TimeSpan Time { get; }

        #endregion
    }
}
