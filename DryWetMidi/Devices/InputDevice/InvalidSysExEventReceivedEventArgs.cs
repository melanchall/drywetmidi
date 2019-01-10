using System;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Provides data for the <see cref="InputDevice.InvalidSysExEventReceived"/> event.
    /// </summary>
    public sealed class InvalidSysExEventReceivedEventArgs : EventArgs
    {
        #region Constructor

        internal InvalidSysExEventReceivedEventArgs(byte[] data)
        {
            Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data of invalid system exclusive event received by <see cref="InputDevice"/>.
        /// </summary>
        public byte[] Data { get; }

        #endregion
    }
}
