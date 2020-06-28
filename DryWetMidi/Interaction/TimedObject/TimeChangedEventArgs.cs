using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Contains arguments for the <see cref="INotifyTimeChanged.TimeChanged"/> event.
    /// </summary>
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

        /// <summary>
        /// Gets the old time of an object.
        /// </summary>
        public long OldTime { get; }

        /// <summary>
        /// Gets the new time of an object.
        /// </summary>
        public long NewTime { get; }

        #endregion
    }
}
