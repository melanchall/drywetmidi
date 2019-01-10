using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Provides a way to connect an input MIDI device to an output MIDI device.
    /// </summary>
    public sealed class DevicesConnector : IDisposable
    {
        #region Fields

        private bool _disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicesConnector"/> with the specified
        /// input and output MIDI devices.
        /// </summary>
        /// <remarks>
        /// <paramref name="inputDevice"/> will not be actually connected to <paramref name="outputDevice"/> after
        /// an instance of <see cref="DevicesConnector"/> is created. You must call <see cref="Connect"/> method
        /// to establish connection between devices.
        /// </remarks>
        /// <param name="inputDevice">Input MIDI device to connect to <paramref name="outputDevice"/>.</param>
        /// <param name="outputDevice">Output MIDI device to connect <paramref name="inputDevice"/> to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="inputDevice"/> is null. -or-
        /// <paramref name="outputDevice"/> is null.</exception>
        public DevicesConnector(InputDevice inputDevice, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(inputDevice), inputDevice);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            InputDevice = inputDevice;
            OutputDevice = outputDevice;
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Finalizes the current instance of the <see cref="DevicesConnector"/>.
        /// </summary>
        ~DevicesConnector()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets an input MIDI device to connect to <see cref="OutputDevice"/>.
        /// </summary>
        public InputDevice InputDevice { get; }

        /// <summary>
        /// Gets an output MIDI device to connect <see cref="InputDevice"/> to.
        /// </summary>
        public OutputDevice OutputDevice { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Connects <see cref="InputDevice"/> to <see cref="OutputDevice"/>.
        /// </summary>
        /// <exception cref="MidiDeviceException"><see cref="InputDevice"/> is already connected
        /// to <see cref="OutputDevice"/>.</exception>
        public void Connect()
        {
            var result = MidiConnectWinApi.midiConnect(InputDevice.GetHandle(), OutputDevice.GetHandle(), IntPtr.Zero);
            if (result == MidiWinApi.MIDIERR_NOTREADY)
                throw new MidiDeviceException("Specified input device is already connected to an output device.");
        }

        /// <summary>
        /// Disconnects <see cref="InputDevice"/> from <see cref="OutputDevice"/>.
        /// </summary>
        public void Disconnect()
        {
            MidiConnectWinApi.midiDisconnect(InputDevice.GetHandle(), OutputDevice.GetHandle(), IntPtr.Zero);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current instance of <see cref="DevicesConnector"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
            }

            Disconnect();

            _disposed = true;
        }

        #endregion
    }
}
