using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class VirtualDevice : MidiDevice
    {
        #region Fields

        private VirtualDeviceApi.Callback_Mac _callback_Mac;
        private VirtualDeviceApi.Callback_Te _callback_Te;

        #endregion

        #region Constructor

        internal VirtualDevice(string name)
            : base()
        {
            Name = name;

            var apiType = CommonApiProvider.Api.Api_GetApiType();
            switch (apiType)
            {
                case CommonApi.API_TYPE.API_TYPE_MAC:
                    InitializeDevice_Mac();
                    break;
                case CommonApi.API_TYPE.API_TYPE_WIN:
                    InitializeDevice_Win();
                    break;
                default:
                    throw new NotSupportedException($"{apiType} API is not supported.");
            }
        }

        #endregion

        #region Properties

        public InputDevice InputDevice { get; private set; }

        public OutputDevice OutputDevice { get; private set; }

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

        private void OnMessage_Mac(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon)
        {
            var result = VirtualDeviceApiProvider.Api.Api_SendDataBack(pktlist, readProcRefCon);
            if (result != VirtualDeviceApi.VIRTUAL_SENDBACKRESULT.VIRTUAL_SENDBACKRESULT_OK)
            {
                var exception = new MidiDeviceException($"Failed to send data back ({result}).", (int)result);
                OnError(exception);
            }
        }

        private void OnMessage_Te(IntPtr midiPort, IntPtr midiDataBytes, uint length, IntPtr dwCallbackInstance)
        {
            var result = VirtualDeviceApiProvider.Api.Api_SendDataBack_Te(midiPort, midiDataBytes, length);
            if (result != VirtualDeviceApi.VIRTUAL_SENDBACKRESULT.VIRTUAL_SENDBACKRESULT_OK)
            {
                var exception = new MidiDeviceException($"Failed to send data back ({result}).", (int)result);
                OnError(exception);
            }
        }

        private void InitializeDevice_Mac()
        {
            var sessionHandle = MidiDevicesSession.GetSessionHandle();

            _callback_Mac = OnMessage_Mac;
            NativeApi.HandleResult(
                VirtualDeviceApiProvider.Api.Api_OpenDevice_Mac(Name, sessionHandle, _callback_Mac, out _info));

            var inputDeviceInfo = VirtualDeviceApiProvider.Api.Api_GetInputDeviceInfo(_info);
            InputDevice = new InputDevice(inputDeviceInfo, DeviceOwner.VirtualDevice);

            var outputDeviceInfo = VirtualDeviceApiProvider.Api.Api_GetOutputDeviceInfo(_info);
            OutputDevice = new OutputDevice(outputDeviceInfo, DeviceOwner.VirtualDevice);
        }

        private void InitializeDevice_Win()
        {
            var sessionHandle = MidiDevicesSession.GetSessionHandle();

            _callback_Te = OnMessage_Te;
            NativeApi.HandleResult(
                VirtualDeviceApiProvider.Api.Api_OpenDevice_Te(Name, sessionHandle, _callback_Te, out _info));

            IntPtr inputDeviceInfo;
            var inputDevicesCount = InputDeviceApiProvider.Api.Api_GetDevicesCount();
            InputDeviceApiProvider.Api.Api_GetDeviceInfo(inputDevicesCount - 1, out inputDeviceInfo);
            InputDevice = new InputDevice(inputDeviceInfo, DeviceOwner.VirtualDevice);
            if (InputDevice.Name != Name)
                throw new MidiDeviceException("Failed to initialize input subdevice due to names mismatch.");

            IntPtr outputDeviceInfo;
            var outputDevicesCount = OutputDeviceApiProvider.Api.Api_GetDevicesCount();
            OutputDeviceApiProvider.Api.Api_GetDeviceInfo(outputDevicesCount - 1, out outputDeviceInfo);
            OutputDevice = new OutputDevice(outputDeviceInfo, DeviceOwner.VirtualDevice);
            if (OutputDevice.Name != Name)
                throw new MidiDeviceException("Failed to initialize output subdevice due to names mismatch.");
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

            if (_info != IntPtr.Zero)
            {
                InputDevice.Dispose(disposing);
                OutputDevice.Dispose(disposing);

                VirtualDeviceApiProvider.Api.Api_CloseDevice(_info);
                _info = IntPtr.Zero;
            }

            _disposed = true;
        }

        #endregion
    }
}
