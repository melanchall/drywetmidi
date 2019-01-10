using System;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Provides data for the <see cref="InputDevice.InvalidShortEventReceived"/> event.
    /// </summary>
    public sealed class InvalidShortEventReceivedEventArgs : EventArgs
    {
        #region Constructor

        internal InvalidShortEventReceivedEventArgs(byte statusByte, byte firstDataByte, byte secondDataByte)
        {
            StatusByte = statusByte;
            FirstDataByte = firstDataByte;
            SecondDataByte = secondDataByte;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the status byte of an invalid MIDI event.
        /// </summary>
        public byte StatusByte { get; }

        /// <summary>
        /// Gets the first data byte of an invalid MIDI event.
        /// </summary>
        public byte FirstDataByte { get; }

        /// <summary>
        /// Gets the second data byte of an invalid MIDI event.
        /// </summary>
        public byte SecondDataByte { get; }

        #endregion
    }
}
