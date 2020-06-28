using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides the event which fired when the time of an object has been changed.
    /// </summary>
    public interface INotifyTimeChanged
    {
        #region Events

        /// <summary>
        /// Occurs when the time of an object has been changed.
        /// </summary>
        event EventHandler<TimeChangedEventArgs> TimeChanged;

        #endregion
    }
}
