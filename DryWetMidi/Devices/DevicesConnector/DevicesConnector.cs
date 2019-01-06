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
            ProcessMmResult(MidiConnectWinApi.midiConnect(InputDevice.GetHandle(), OutputDevice.GetHandle(), IntPtr.Zero));
        }

        public void Disconnect()
        {
            ProcessMmResult(MidiConnectWinApi.midiDisconnect(InputDevice.GetHandle(), OutputDevice.GetHandle(), IntPtr.Zero));
        }

        private static void ProcessMmResult(uint mmResult)
        {
            switch (mmResult)
            {
                case MidiWinApi.MMSYSERR_INVALHANDLE:
                    throw new MidiDeviceException("Specified device handle is invalid.");
                case MidiWinApi.MIDIERR_NOTREADY:
                    throw new MidiDeviceException("Specified input device is already connected to an output device.");
            }
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
            }

            Disconnect();

            _disposed = true;
        }

        #endregion
    }
}
