using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents a virtual loopback MIDI device (MIDI cable). More info in the
    /// <see href="xref:a_dev_virtual">Virtual device</see> article.
    /// </summary>
    public sealed class VirtualDevice : MidiDevice
    {
        #region Nested classes

        private sealed class VirtualDeviceHandle : NativeHandle
        {
#if TEST
            private readonly TestCheckpoints _checkpoints;

            public VirtualDeviceHandle(IntPtr validHandle, TestCheckpoints checkpoints)
                : this(validHandle)
            {
                _checkpoints = checkpoints;
            }
#endif

            public VirtualDeviceHandle(IntPtr validHandle)
                : base(validHandle)
            {
            }

            protected override bool ReleaseHandle()
            {
#if TEST
                _checkpoints?.SetCheckpointReached(VirtualDeviceCheckpointsNames.HandleFinalizerEntered);
#endif

                var closeResult = VirtualDeviceApiProvider.Api.Api_CloseDevice(handle);
                if (closeResult != VirtualDeviceApi.VIRTUAL_CLOSERESULT.VIRTUAL_CLOSERESULT_OK)
                    return false;

#if TEST
                _checkpoints?.SetCheckpointReached(VirtualDeviceCheckpointsNames.DeviceClosedInHandleFinalizer);
#endif

                return true;
            }
        }

        #endregion

        #region Fields

        private readonly string _name;

        private VirtualDeviceApi.Callback_Mac _callback_Mac;

        private VirtualDeviceHandle _handle = null;

        #endregion

        #region Constructor

        internal VirtualDevice(string name)
            : base(CreationContext.User)
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

        /// <summary>
        /// Gets the name of the current MIDI device.
        /// </summary>
        public override string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the input subdevice of the current <see cref="VirtualDevice"/>.
        /// </summary>
        public InputDevice InputDevice { get; private set; }

        /// <summary>
        /// Gets the output subdevice of the current <see cref="VirtualDevice"/>.
        /// </summary>
        public OutputDevice OutputDevice { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the <see cref="VirtualDevice"/> with the specified name.
        /// </summary>
        /// <param name="name">The name of a virtual device to create.</param>
        /// <returns>An instance of the <see cref="VirtualDevice"/> with name of <paramref name="name"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="NotSupportedException">Virtual device creation is not supported on the current operating system.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device creation.</exception>
        public static VirtualDevice Create(string name)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Device name");

            var apiType = CommonApiProvider.Api.Api_GetApiType();
            if (apiType != CommonApi.API_TYPE.API_TYPE_MAC)
                throw new NotSupportedException("Virtual device creation is not supported on the current operating system.");

            return new VirtualDevice(name);
        }

        private void OnMessage_Mac(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon)
        {
            if (!IsEnabled)
                return;

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

            var deviceInfo = IntPtr.Zero;
            NativeApiUtilities.HandleDevicesNativeApiResult(
                VirtualDeviceApiProvider.Api.Api_OpenDevice_Mac(Name, sessionHandle, _callback_Mac, out deviceInfo));

            var inputDeviceInfo = VirtualDeviceApiProvider.Api.Api_GetInputDeviceInfo(deviceInfo);
            InputDevice = new InputDevice(inputDeviceInfo, CreationContext.VirtualDevice);

            var outputDeviceInfo = VirtualDeviceApiProvider.Api.Api_GetOutputDeviceInfo(deviceInfo);
            OutputDevice = new OutputDevice(outputDeviceInfo, CreationContext.VirtualDevice);

#if TEST
            _handle = new VirtualDeviceHandle(deviceInfo, TestCheckpoints);
#else
            _handle = new VirtualDeviceHandle(deviceInfo);
#endif
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
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

            if (disposing)
            {
                InputDevice?.Dispose(true);
                OutputDevice?.Dispose(true);
                _handle?.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
