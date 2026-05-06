using System;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Common;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Configuration
{
    internal static partial class MidiConfigurationApi
    {
        #region Nested enums

        public enum CONFIGURATION_GETRESULT
        {
            CONFIGURATION_GETRESULT_OK = 0,

            CONFIGURATION_GETRESULT_CANTCREATEWMSSDKINITIALIZER = 1,
            CONFIGURATION_GETRESULT_CANTINITIALIZEWMSSDK = 2,
            CONFIGURATION_GETRESULT_OLDWMSSDK = 3,
            CONFIGURATION_GETRESULT_WMSSERVICEUNAVAILABLE = 4,
            CONFIGURATION_GETRESULT_WMSUNKNOWNERROR = 5,
        }

        public enum CONFIGURATION_CLEANUPRESULT
        {
            CONFIGURATION_CLEANUPRESULT_OK = 0,

            CONFIGURATION_CLEANUPRESULT_WMSUNKNOWNERROR = 1,
        }

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial CONFIGURATION_GETRESULT GetConfiguration_Win([MarshalAs(UnmanagedType.U1)] bool useWms, out IntPtr configuration, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial CONFIGURATION_GETRESULT GetConfiguration_Mac(out IntPtr configuration, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial CONFIGURATION_CLEANUPRESULT CleanupConfiguration(IntPtr configuration);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.U1)]
        private static partial bool IsVirtualDeviceApiAvailable(MidiConfigurationHandle configuration);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.U1)]
        private static partial bool IsDevicesCachingRequired(MidiConfigurationHandle configuration);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern CONFIGURATION_GETRESULT GetConfiguration_Win([MarshalAs(UnmanagedType.U1)] bool useWms, out IntPtr configuration, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern CONFIGURATION_GETRESULT GetConfiguration_Mac(out IntPtr configuration, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern CONFIGURATION_CLEANUPRESULT CleanupConfiguration(IntPtr configuration);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool IsVirtualDeviceApiAvailable(MidiConfigurationHandle configuration);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool IsDevicesCachingRequired(MidiConfigurationHandle configuration);
#endif

        #endregion

        #region Methods

        public static CONFIGURATION_GETRESULT Api_GetConfiguration(bool useWms, out IntPtr configuration, out int errorCode)
        {
            switch (CommonApi.Api_GetApiType())
            {
                case CommonApi.API_TYPE.API_TYPE_WIN:
                    return GetConfiguration_Win(useWms, out configuration, out errorCode);
                case CommonApi.API_TYPE.API_TYPE_MAC:
                    return GetConfiguration_Mac(out configuration, out errorCode);
            }

            // TODO: message
            throw new NotSupportedException();
        }

        public static CONFIGURATION_CLEANUPRESULT Api_CleanupConfiguration(IntPtr configuration)
        {
            return CleanupConfiguration(configuration);
        }

        public static bool Api_IsVirtualDeviceApiAvailable(MidiConfigurationHandle configuration)
        {
            return IsVirtualDeviceApiAvailable(configuration);
        }

        public static bool Api_IsDevicesCachingRequired(MidiConfigurationHandle configuration)
        {
            return IsDevicesCachingRequired(configuration);
        }

        #endregion
    }
}
