using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class MidiDevicesSession
    {
        #region Events

        internal static event EventHandler<IntPtr> InputDeviceAdded;
        internal static event EventHandler<IntPtr> InputDeviceRemoved;
        internal static event EventHandler<IntPtr> OutputDeviceAdded;
        internal static event EventHandler<IntPtr> OutputDeviceRemoved;

        #endregion

        #region Fields

        private static readonly object _lockObject = new object();

        private static MidiDevicesSessionHandle _handle;

        private static MidiDevicesSessionApi.InputDeviceCallback _inputDeviceCallback;
        private static MidiDevicesSessionApi.OutputDeviceCallback _outputDeviceCallback;

        #endregion

        #region Properties

#if TEST
        internal static TestCheckpoints TestCheckpoints { get; set; }
#endif

        #endregion

        #region Methods

        public static MidiDevicesSessionHandle GetSessionHandle()
        {
            NativeApiUtilities.EnsureOsIsSupported();

            if (_handle == null || _handle.IsInvalid)
            {
                lock (_lockObject)
                {
                    if (_handle == null || _handle.IsInvalid)
                    {
                        _inputDeviceCallback = InputDeviceCallback;
                        _outputDeviceCallback = OutputDeviceCallback;

                        var openResult = MidiDevicesSessionApi.Api_OpenSession(Guid.NewGuid().ToString(), MidiConfiguration.GetConfigurationHandle(), _inputDeviceCallback, _outputDeviceCallback, out var rawHandle, out var errorCode);
                        MidiDeviceUtilities.HandleDevicesNativeApiResult(openResult, errorCode);

                        _handle = new MidiDevicesSessionHandle(rawHandle);
                        _handle.IsDevicesCachingRequired = MidiConfigurationApi.Api_IsDevicesCachingRequired(MidiConfiguration.GetConfigurationHandle());

#if TEST
                        _handle.TestCheckpoints = TestCheckpoints;
#endif

                        AppDomain.CurrentDomain.DomainUnload += OnDomainUnloadOrExit;
                        AppDomain.CurrentDomain.ProcessExit += OnDomainUnloadOrExit;
                    }
                }
            }

            return _handle;
        }

        private static void OnDomainUnloadOrExit(object sender, EventArgs e)
        {
            if (_handle != null && !_handle.IsInvalid)
            {
                lock (_lockObject)
                {
                    if (_handle != null && !_handle.IsInvalid)
                    {
                        _handle?.Dispose();
                        _handle = null;
                    }
                }
            }
        }

        private static void InputDeviceCallback(IntPtr info, bool operation)
        {
            if (operation)
                InputDeviceAdded?.Invoke(null, info);
            else
                InputDeviceRemoved?.Invoke(null, info);
        }

        private static void OutputDeviceCallback(IntPtr info, bool operation)
        {
            if (operation)
                OutputDeviceAdded?.Invoke(null, info);
            else
                OutputDeviceRemoved?.Invoke(null, info);
        }

        #endregion
    }
}
