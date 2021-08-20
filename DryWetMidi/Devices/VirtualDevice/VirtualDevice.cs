using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class VirtualDevice : MidiDevice
    {
        #region Fields

        private readonly string _name;

        private VirtualDeviceApi.Callback_Mac _callback_Mac;

        #endregion

        #region Constructor

        internal VirtualDevice(string name)
            : base(IntPtr.Zero, CreationContext.User)
        {
            _name = name;

            var apiType = CommonApiProvider.Api.Api_GetApiType();
            switch (apiType)
            {
                case CommonApi.API_TYPE.API_TYPE_MAC:
                    InitializeDevice_Mac();
                    break;
            }
        }

        #endregion

        #region Properties

        public override string Name
        {
            get { return _name; }
        }

        public InputDevice InputDevice { get; private set; }

        public OutputDevice OutputDevice { get; private set; }

        #endregion

        #region Methods

        public static VirtualDevice Create(string name)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Device name");

            var apiType = CommonApiProvider.Api.Api_GetApiType();
            if (apiType != CommonApi.API_TYPE.API_TYPE_MAC)
                throw new NotSupportedException($"Virtual device creation is not supported for the {apiType} API.");

            return new VirtualDevice(name);
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

        private void InitializeDevice_Mac()
        {
            var sessionHandle = MidiDevicesSession.GetSessionHandle();

            _callback_Mac = OnMessage_Mac;
            NativeApi.HandleResult(
                VirtualDeviceApiProvider.Api.Api_OpenDevice_Mac(Name, sessionHandle, _callback_Mac, out _info));

            var inputDeviceInfo = VirtualDeviceApiProvider.Api.Api_GetInputDeviceInfo(_info);
            InputDevice = new InputDevice(inputDeviceInfo, CreationContext.VirtualDevice);

            var outputDeviceInfo = VirtualDeviceApiProvider.Api.Api_GetOutputDeviceInfo(_info);
            OutputDevice = new OutputDevice(outputDeviceInfo, CreationContext.VirtualDevice);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return "Virtual device";
        }

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
