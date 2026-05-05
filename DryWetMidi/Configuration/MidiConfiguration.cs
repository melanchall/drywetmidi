using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using System;

namespace Melanchall.DryWetMidi.Configuration
{
    internal static class MidiConfiguration
    {
        #region Fields

        private static readonly object _lockObject = new object();

        private static MidiConfigurationHandle _handle;

        #endregion

        #region Properties

#if TEST
        internal static TestCheckpoints TestCheckpoints { get; set; }
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
                        int errorCode = 0;
                        var rawHandle = IntPtr.Zero;

                        // TODO: pass useWms
                        var result = MidiConfigurationApi.Api_GetConfiguration(true, out rawHandle, out errorCode);
                        
                        // TODO: separate from devices
                        MidiDeviceUtilities.HandleDevicesNativeApiResult(result, errorCode);

                        _handle = new MidiConfigurationHandle(rawHandle);

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

        #endregion
    }
}
