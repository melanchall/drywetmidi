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

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NativeApiActivityCallback(IntPtr record);

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial CONFIGURATION_GETRESULT GetConfiguration_Win([MarshalAs(UnmanagedType.U1)] bool useWms, NativeApiActivityCallback activityCallback, out IntPtr configuration, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial CONFIGURATION_GETRESULT GetConfiguration_Mac(NativeApiActivityCallback activityCallback, out IntPtr configuration, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial CONFIGURATION_CLEANUPRESULT CleanupConfiguration(IntPtr configuration);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial ApiType GetApiType(MidiConfigurationHandle configuration);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.U1)]
        private static partial bool IsVirtualDeviceApiAvailable(MidiConfigurationHandle configuration);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.U1)]
        private static partial bool IsDevicesWatcherApiAvailable(MidiConfigurationHandle configuration);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.U1)]
        private static partial bool IsWmsInitialized(MidiConfigurationHandle configuration);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void CheckNativeApiActivityCallback(MidiConfigurationHandle configuration);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void CheckWinRtErrorHandling_Win(MidiConfigurationHandle configuration);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void CheckStdExceptionHandling_Win(MidiConfigurationHandle configuration);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern CONFIGURATION_GETRESULT GetConfiguration_Win([MarshalAs(UnmanagedType.U1)] bool useWms, NativeApiActivityCallback activityCallback, out IntPtr configuration, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern CONFIGURATION_GETRESULT GetConfiguration_Mac(NativeApiActivityCallback activityCallback, out IntPtr configuration, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern CONFIGURATION_CLEANUPRESULT CleanupConfiguration(IntPtr configuration);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern ApiType GetApiType(MidiConfigurationHandle configuration);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool IsVirtualDeviceApiAvailable(MidiConfigurationHandle configuration);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool IsDevicesWatcherApiAvailable(MidiConfigurationHandle configuration);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool IsWmsInitialized(MidiConfigurationHandle configuration);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CheckNativeApiActivityCallback(MidiConfigurationHandle configuration);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CheckWinRtErrorHandling_Win(MidiConfigurationHandle configuration);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CheckStdExceptionHandling_Win(MidiConfigurationHandle configuration);
#endif

        #endregion

        #region Methods

        public static CONFIGURATION_GETRESULT Api_GetConfiguration(
            bool useWms,
            NativeApiActivityCallback activityCallback,
            out IntPtr configuration,
            out int errorCode)
        {
            var osType = CommonApi.Api_GetOsType();
            switch (osType)
            {
                case CommonApi.OsType.Windows:
                    return GetConfiguration_Win(useWms, activityCallback, out configuration, out errorCode);
                case CommonApi.OsType.MacOS:
                    return GetConfiguration_Mac(activityCallback, out configuration, out errorCode);
            }

            throw new NotImplementedException($"OS type {osType} not supported.");
        }

        public static CONFIGURATION_CLEANUPRESULT Api_CleanupConfiguration(IntPtr configuration)
        {
            return CleanupConfiguration(configuration);
        }

        public static ApiType Api_GetApiType(MidiConfigurationHandle configuration)
        {
            return GetApiType(configuration);
        }

        public static bool Api_IsVirtualDeviceApiAvailable(MidiConfigurationHandle configuration)
        {
            return IsVirtualDeviceApiAvailable(configuration);
        }

        public static bool Api_IsDevicesWatcherApiAvailable(MidiConfigurationHandle configuration)
        {
            return IsDevicesWatcherApiAvailable(configuration);
        }

        public static bool Api_IsWmsInitialized(MidiConfigurationHandle configuration)
        {
            return IsWmsInitialized(configuration);
        }

        public static void Api_CheckNativeApiActivityCallback(MidiConfigurationHandle configuration)
        {
            CheckNativeApiActivityCallback(configuration);
        }

        public static void Api_CheckWinRtErrorHandling_Win(MidiConfigurationHandle configuration)
        {
            CheckWinRtErrorHandling_Win(configuration);
        }

        public static void Api_CheckStdExceptionHandling_Win(MidiConfigurationHandle configuration)
        {
            CheckStdExceptionHandling_Win(configuration);
        }

        #endregion
    }
}
