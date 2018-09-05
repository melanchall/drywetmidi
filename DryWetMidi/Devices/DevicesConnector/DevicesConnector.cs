using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class DevicesConnector : IDisposable
    {
        #region Fields

        private bool _disposed = false;

        #endregion

        #region Constructor

        public DevicesConnector(InputDevice inputDevice, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(inputDevice), inputDevice);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            InputDevice = inputDevice;
            OutputDevice = outputDevice;
        }

        #endregion

        #region Finalizer

        ~DevicesConnector()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public InputDevice InputDevice { get; }

        public OutputDevice OutputDevice { get; }

        #endregion

        #region Methods

        public void Connect()
        {
            MidiConnectWinApi.midiConnect(InputDevice.GetHandle(), OutputDevice.GetHandle(), IntPtr.Zero);
        }

        public void Disconnect()
        {
            MidiConnectWinApi.midiDisconnect(InputDevice.GetHandle(), OutputDevice.GetHandle(), IntPtr.Zero);
        }

        #endregion

        #region IDisposable Support

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
                // TODO: dispose managed state (managed objects).
            }

            Disconnect();

            _disposed = true;
        }

        #endregion
    }
}
