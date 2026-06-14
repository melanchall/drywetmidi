using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents a virtual loopback MIDI device (MIDI cable). More info in the
    /// <see href="xref:a_dev_virtual">Virtual device</see> article.
    /// </summary>
    /// <remarks>
    /// <os-specific-api/>
    /// <advanced-windows-api/>
    /// </remarks>
    public sealed class VirtualDevice : IDisposable
    {
        #region Events

        public event EventHandler<ErrorOccurredEventArgs> ErrorOccurred;

        #endregion

        #region Fields

        private readonly string _name;

        private VirtualDeviceHandle _handle;
        private bool _disposed = false;
        private bool _enabled = true;

        private VirtualDeviceApi.Callback_Mac _callbackMac;

        private InputEndpoint _inputEndpoint;
        private OutputEndpoint _outputEndpoint;

#if TEST
        private TestCheckpoints _testCheckpoints;
#endif

        #endregion

        #region Constructor

        internal VirtualDevice(string name)
        {
            _name = name;

            var apiType = CommonApi.Api_GetApiType();
            switch (apiType)
            {
                case CommonApi.API_TYPE.API_TYPE_MAC:
                    InitializeDevice_Mac();
                    break;
                case CommonApi.API_TYPE.API_TYPE_WIN:
                    InitializeDevice_Win();
                    break;
            }
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Finalizes the current instance of the virtual MIDI device class.
        /// </summary>
        ~VirtualDevice()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the current virtual MIDI device.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        public bool IsEnabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;

                var configuration = MidiConfiguration.GetConfigurationHandle();

                if (_enabled)
                    VirtualDeviceApi.Api_UnmuteDevice(_handle, configuration);
                else
                    VirtualDeviceApi.Api_MuteDevice(_handle, configuration);
            }
        }

        /// <summary>
        /// Gets the input endpoint of the current <see cref="VirtualDevice"/>.
        /// </summary>
        public InputEndpoint InputEndpoint
        {
            get
            {
                EnsureDeviceIsNotDisposed();

                return _inputEndpoint;
            }
            private set { _inputEndpoint = value; }
        }

        /// <summary>
        /// Gets the output endpoint of the current <see cref="VirtualDevice"/>.
        /// </summary>
        public OutputEndpoint OutputEndpoint
        {
            get
            {
                EnsureDeviceIsNotDisposed();

                return _outputEndpoint;
            }
            private set { _outputEndpoint = value; }
        }

#if TEST
        internal TestCheckpoints TestCheckpoints
        {
            get { return _testCheckpoints; }
            set
            {
                _testCheckpoints = value;

                if (_handle != null)
                    _handle.TestCheckpoints = value;
            }
        }
#endif

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the <see cref="VirtualDevice"/> with the specified name.
        /// </summary>
        /// <param name="name">The name of a virtual device to create.</param>
        /// <returns>An instance of the <see cref="VirtualDevice"/> with name of <paramref name="name"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="NativeApiException">An error occurred on device creation.</exception>
        public static VirtualDevice Create(string name)
        {
            NativeApiUtilities.EnsureOsIsSupported();

            // TODO: choose exception type and document it above
            if (!LibraryConfiguration.IsVirtualDeviceApiAvailable())
                throw new FeatureNotAvailableException("Virtual device API is not available.");

            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Device name");

            return new VirtualDevice(name);
        }

        private void EnsureDeviceIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Device is disposed.");
        }

        private void OnError(Exception exception)
        {
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(exception));
        }

        private void OnMessage_Mac(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon)
        {
            if (!IsEnabled)
                return;

            var result = VirtualDeviceApi.Api_SendDataBack(pktlist, readProcRefCon, out var errorCode);
            if (result != VirtualDeviceApi.VIRTUAL_SENDBACKRESULT.VIRTUAL_SENDBACKRESULT_OK)
            {
                var exception = new NativeApiException($"Failed to send data back ({result}).", (int)result, errorCode);
                OnError(exception);
            }
        }

        private void InitializeDevice_Mac()
        {
            var sessionHandle = MidiDevicesSession.GetSessionHandle();
            var configuration = MidiConfiguration.GetConfigurationHandle();

            _callbackMac = OnMessage_Mac;

            var result = VirtualDeviceApi.Api_OpenDevice_Mac(Name, configuration, sessionHandle, _callbackMac, out var deviceInfo, out var errorCode);
            NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);

            InitializeDevice(deviceInfo);
        }

        private void InitializeDevice_Win()
        {
            var sessionHandle = MidiDevicesSession.GetSessionHandle();
            var configuration = MidiConfiguration.GetConfigurationHandle();

            var result = VirtualDeviceApi.Api_OpenDevice_Win(Name, configuration, sessionHandle, out var deviceInfo, out var errorCode);
            NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);

            InitializeDevice(deviceInfo);
        }

        private void InitializeDevice(IntPtr deviceInfo)
        {
            var inputEndpointInfo = VirtualDeviceApi.Api_GetInputEndpointInfo(deviceInfo);
            InputEndpoint = new InputEndpoint(inputEndpointInfo, MidiEndpoint.CreationContext.VirtualDevice);

            var outputEndpointInfo = VirtualDeviceApi.Api_GetOutputEndpointInfo(deviceInfo);
            OutputEndpoint = new OutputEndpoint(outputEndpointInfo, MidiEndpoint.CreationContext.VirtualDevice);

            _handle = new VirtualDeviceHandle(deviceInfo);

#if TEST
            _handle.TestCheckpoints = TestCheckpoints;
            InputEndpoint.TestCheckpoints = TestCheckpoints;
            OutputEndpoint.TestCheckpoints = TestCheckpoints;
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
            return $"Virtual device ({_name})";
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the MIDI device class instance.
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

            _handle?.Dispose();
            _handle = null;

            if (disposing)
            {
                InputEndpoint?.Dispose(true);
                OutputEndpoint?.Dispose(true);
            }

            _disposed = true;
        }

        #endregion
    }
}
