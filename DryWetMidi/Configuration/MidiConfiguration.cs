using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Configuration
{
    internal static class MidiConfiguration
    {
        private const string AppLocalWmsBootstrapEnvironmentVariableName = "DRYWETMIDI_ENABLE_APPLOCAL_WMS_BOOTSTRAP";

        #region Events

        internal static event EventHandler<string>? NativeApiMessageReceived;

        #endregion

        #region Fields

#if NET9_0_OR_GREATER
        private static readonly System.Threading.Lock _lockObject = new();
#else
        private static readonly object _lockObject = new();
#endif

        private static MidiConfigurationHandle? _handle;
        private static MidiConfigurationApi.NativeApiActivityCallback? _nativeApiActivityCallback;

        [ThreadStatic]
        private static List<string>? _deferredActivityMessages;

        #endregion

        #region Properties

        public static bool UseWindowsMidiServices { get; set; } = true;

        internal static List<string>? DeferredActivityMessages
        {
            get { return _deferredActivityMessages; }
            set { _deferredActivityMessages = value; }
        }

#if TEST
        internal static TestCheckpoints? TestCheckpoints { get; set; }
#endif

        #endregion

        #region Methods

        public static MidiConfigurationHandle GetConfigurationHandle()
        {
            NativeApiUtilities.EnsureOsIsSupported();

            if (_handle == null || _handle.IsInvalid)
            {
                lock (_lockObject)
                {
                    if (_handle == null || _handle.IsInvalid)
                    {
                        var errorCode = 0;
                        var rawHandle = IntPtr.Zero;
                        var enableAppLocalWmsBootstrap = IsAppLocalWmsBootstrapEnabled();

                        _nativeApiActivityCallback = NativeApiActivityCallback;
                        var result = MidiConfigurationApi.Api_GetConfiguration(UseWindowsMidiServices, enableAppLocalWmsBootstrap, _nativeApiActivityCallback, out rawHandle, out errorCode);
                        NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);


                        _handle = new MidiConfigurationHandle(rawHandle);

                        var apiType = MidiConfigurationApi.Api_GetApiType(_handle);
                        if (apiType != ApiType.WindowsMidiServices)
                            MidiOperationsExecutor.Instance.UseDirectExecution();

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

        internal static void ResetHandle()
        {
            lock (_lockObject)
            {
                if (_handle != null)
                {
                    _handle.Dispose();
                    _handle = null;
                }
            }
        }

        private static void OnDomainUnloadOrExit(object? sender, EventArgs e)
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

        internal static TResult ExecuteWithDeferredActivityMessages<TResult>(Func<TResult> operation, List<string>? messages)
        {
            if (messages == null)
                return operation();

            var previousMessages = _deferredActivityMessages;
            _deferredActivityMessages = messages;

            try
            {
                return operation();
            }
            finally
            {
                _deferredActivityMessages = previousMessages;
            }
        }

        internal static void ReportDeferredActivityMessages(List<string> messages, List<string>? previousMessages)
        {
            _deferredActivityMessages = previousMessages;

            if (previousMessages != null)
            {
                previousMessages.AddRange(messages);
                return;
            }

            foreach (var message in messages)
            {
                NativeApiMessageReceived?.Invoke(null, message);
            }
        }

        private static void NativeApiActivityCallback(IntPtr record)
        {
            var text = NativeApi.GetStringFromPointer(record);

            if (_deferredActivityMessages != null)
                _deferredActivityMessages.Add(text);
            else
                NativeApiMessageReceived?.Invoke(null, text);
        }

        private static bool IsAppLocalWmsBootstrapEnabled()
        {
            var value = Environment.GetEnvironmentVariable(AppLocalWmsBootstrapEnvironmentVariableName);
            return
                string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "1", StringComparison.Ordinal);
        }

        #endregion
    }
}
