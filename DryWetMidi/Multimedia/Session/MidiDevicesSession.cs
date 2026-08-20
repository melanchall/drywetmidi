using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class MidiDevicesSession
    {
        #region Events

        internal static event EventHandler<IntPtr>? InputEndpointAdded;
        internal static event EventHandler<IntPtr>? InputEndpointRemoved;
        internal static event EventHandler<IntPtr>? OutputEndpointAdded;
        internal static event EventHandler<IntPtr>? OutputEndpointRemoved;

        #endregion

        #region Fields

        private static readonly object _lockObject = new object();

        private static MidiDevicesSessionHandle? _handle;

        private static MidiDevicesSessionApi.InputEndpointCallback? _inputEndpointCallback;
        private static MidiDevicesSessionApi.OutputEndpointCallback? _outputEndpointCallback;

        #endregion

        #region Properties

#if TEST
        internal static TestCheckpoints? TestCheckpoints { get; set; }
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
                        _inputEndpointCallback = InputEndpointCallback;
                        _outputEndpointCallback = OutputEndpointCallback;

                        var openResult = MidiDevicesSessionApi.Api_OpenSession($"DryWetMIDI_{Guid.NewGuid()}", MidiConfiguration.GetConfigurationHandle(), _inputEndpointCallback, _outputEndpointCallback, out var rawHandle, out var errorCode);
                        NativeApiUtilities.HandleEndpointNativeApiResult(openResult, errorCode);

                        _handle = new MidiDevicesSessionHandle(rawHandle);

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

        internal static void ResetSessionHandle()
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

        private static void OnDomainUnloadOrExit(object? sender, EventArgs e)
        {
            ResetSessionHandle();
        }

        private static void InputEndpointCallback(IntPtr info, MidiDevicesSessionApi.SESSION_CALLBACKOPERATION operation)
        {
            var handler = operation == MidiDevicesSessionApi.SESSION_CALLBACKOPERATION.SESSION_CALLBACKOPERATION_ENDPOINTADDED
                ? InputEndpointAdded
                : InputEndpointRemoved;

            if (handler != null)
                handler.Invoke(null, info);
            else
                InputEndpointApi.Api_DeleteEndpointInfo(info);
        }

        private static void OutputEndpointCallback(IntPtr info, MidiDevicesSessionApi.SESSION_CALLBACKOPERATION operation)
        {
            var handler = operation == MidiDevicesSessionApi.SESSION_CALLBACKOPERATION.SESSION_CALLBACKOPERATION_ENDPOINTADDED
                ? OutputEndpointAdded
                : OutputEndpointRemoved;

            if (handler != null)
                handler.Invoke(null, info);
            else
                OutputEndpointApi.Api_DeleteEndpointInfo(info);
        }

        #endregion
    }
}
