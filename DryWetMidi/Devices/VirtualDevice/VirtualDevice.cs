using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class VirtualDevice : MidiDevice
    {
        #region Fields

        private VirtualDeviceApi.Callback_Apple _callback_Apple;

        #endregion

        #region Constructor

        internal VirtualDevice(string name)
            : base()
        {
            Name = name;

            var apiType = VirtualDeviceApiProvider.Api.Api_GetApiType();
            var sessionHandle = MidiDevicesSession.GetSessionHandle();

            switch (apiType)
            {
                case VirtualDeviceApi.API_TYPE.API_TYPE_APPLE:
                    {
                        _callback_Apple = OnMessage_Apple;
                        NativeApi.HandleResult(
                            VirtualDeviceApiProvider.Api.Api_OpenDevice_Apple(name, sessionHandle, _callback_Apple, out _info));
                    }
                    break;
                default:
                    throw new NotSupportedException($"{apiType} API is not supported.");
            }

            var inputDeviceInfo = VirtualDeviceApiProvider.Api.Api_GetInputDeviceInfo(_info);
            InputDevice = new InputDevice(inputDeviceInfo, DeviceOwner.VirtualDevice);

            var outputDeviceInfo = VirtualDeviceApiProvider.Api.Api_GetOutputDeviceInfo(_info);
            OutputDevice = new OutputDevice(outputDeviceInfo, DeviceOwner.VirtualDevice);
        }

        #endregion

        #region Properties

        public InputDevice InputDevice { get; }

        public OutputDevice OutputDevice { get; }

        #endregion

        #region Methods

        public static VirtualDevice Create(string name)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Device name");
            
            return new VirtualDevice(name);
        }

        protected override void SetBasicDeviceInformation()
        {
        }

        private void OnMessage_Apple(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon)
        {
            var result = VirtualDeviceApiProvider.Api.Api_SendDataBack(pktlist, readProcRefCon);
            if (result != VirtualDeviceApi.VIRTUAL_SENDBACKRESULT.VIRTUAL_SENDBACKRESULT_OK)
            {
                var exception = new MidiDeviceException($"Failed to send data back ({result}).", (int)result);
                OnError(exception);
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Releases the unmanaged resources used by the MIDI device class and optionally releases
        /// the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to
        /// release only unmanaged resources.</param>
        internal override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            VirtualDeviceApiProvider.Api.Api_CloseDevice(_info);

            InputDevice.Dispose(disposing);
            OutputDevice.Dispose(disposing);

            _disposed = true;
        }

        #endregion
    }
}
