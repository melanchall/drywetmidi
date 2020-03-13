using System;
using System.Collections.Generic;
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
        /// <paramref name="inputDevice"/> will not be actually connected to <paramref name="outputDevices"/> after
        /// an instance of <see cref="DevicesConnector"/> is created. You must call <see cref="Connect"/> method
        /// to establish connection between devices.
        /// </remarks>
        /// <param name="inputDevice">Input MIDI device to connect to <paramref name="outputDevices"/>.</param>
        /// <param name="outputDevices">Output MIDI devices to connect <paramref name="inputDevice"/> to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="inputDevice"/> is null. -or-
        /// <paramref name="outputDevices"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="outputDevices"/> contains null.</exception>
        public DevicesConnector(InputDevice inputDevice, params IOutputDevice[] outputDevices)
        {
            ThrowIfArgument.IsNull(nameof(inputDevice), inputDevice);
            ThrowIfArgument.IsNull(nameof(outputDevices), outputDevices);
            ThrowIfArgument.ContainsNull(nameof(outputDevices), outputDevices);

            InputDevice = inputDevice;
            OutputDevices = outputDevices;
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
        /// Gets an input MIDI device to connect to <see cref="OutputDevices"/>.
        /// </summary>
        public InputDevice InputDevice { get; }

        /// <summary>
        /// Gets output MIDI devices to connect <see cref="InputDevice"/> to.
        /// </summary>
        public IReadOnlyCollection<IOutputDevice> OutputDevices { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Connects <see cref="InputDevice"/> to <see cref="OutputDevices"/>.
        /// </summary>
        public void Connect()
        {
            InputDevice.EventReceived += OnEventReceived;
        }

        /// <summary>
        /// Disconnects <see cref="InputDevice"/> from <see cref="OutputDevices"/>.
        /// </summary>
        public void Disconnect()
        {
            InputDevice.EventReceived -= OnEventReceived;
        }

        private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            foreach (var outputDevice in OutputDevices)
            {
                outputDevice.SendEvent(e.Event);
            }
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
                Disconnect();

            _disposed = true;
        }

        #endregion
    }
}
