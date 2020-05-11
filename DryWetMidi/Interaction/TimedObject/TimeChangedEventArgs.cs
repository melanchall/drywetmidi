using System;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class TimeChangedEventArgs : EventArgs
    {
        #region Constructor

        internal TimeChangedEventArgs(long oldTime, long newTime)
        {
            OldTime = oldTime;
            NewTime = newTime;
        }

        #endregion

        #region Properties

        public long OldTime { get; }

        public long NewTime { get; }

        #endregion
    }
}
