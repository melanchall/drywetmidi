using System;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Tick generator which provides ticking with the specified interval.
    /// </summary>
    public interface ITickGenerator : IDisposable
    {
        #region Events

        /// <summary>
        /// Occurs when new tick generated.
        /// </summary>
        event EventHandler TickGenerated;

        #endregion

        #region Methods

        /// <summary>
        /// Starts the tick generator if it's not started; otherwise does nothing.
        /// </summary>
        void TryStart();

        #endregion
    }
}
