using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides the event which fired when the length of an object has been changed.
    /// </summary>
    public interface INotifyLengthChanged
    {
        #region Events

        /// <summary>
        /// Occurs when the length of an object has been changed.
        /// </summary>
        event EventHandler<LengthChangedEventArgs> LengthChanged;

        #endregion
    }
}
