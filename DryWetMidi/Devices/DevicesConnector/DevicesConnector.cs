using System;
using System.Text;
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
            MidiWinApi.ProcessMmResult(() => MidiConnectWinApi.midiConnect(InputDevice.GetHandle(), OutputDevice.GetHandle(), IntPtr.Zero), GetErrorText);
        }

        public void Disconnect()
        {
            MidiWinApi.ProcessMmResult(() => MidiConnectWinApi.midiDisconnect(InputDevice.GetHandle(), OutputDevice.GetHandle(), IntPtr.Zero), GetErrorText);
        }

        private static uint GetErrorText(uint mmrError, StringBuilder pszText, uint cchText)
        {
            switch (mmrError)
            {
                case MidiWinApi.MMSYSERR_INVALHANDLE:
                    pszText.Append("Specified device handle is invalid.");
                    break;
                case MidiWinApi.MIDIERR_NOTREADY:
                    pszText.Append("Specified input device is already connected to an output device.");
                    break;
            }

            return MidiWinApi.MMSYSERR_NOERROR;
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
