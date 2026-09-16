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
            var osType = MidiSystem.Instance.EnqueueNativeCall(() =>
                CommonApi.Api_GetOsType());

            var configurationLocal = IntPtr.Zero;
            var errorCodeLocal = 0;

            var getResult = MidiSystem.Instance.EnqueueNativeCall(() =>
            {
                switch (osType)
                {
                    case CommonApi.OsType.Windows:
                        return GetConfiguration_Win(useWms, activityCallback, out configurationLocal, out errorCodeLocal);
                    case CommonApi.OsType.MacOS:
                        return GetConfiguration_Mac(activityCallback, out configurationLocal, out errorCodeLocal);
                }

                throw new NotImplementedException($"OS type {osType} not supported.");
            });

            configuration = configurationLocal;
            errorCode = errorCodeLocal;
            return getResult;
        }

        public static CONFIGURATION_CLEANUPRESULT Api_CleanupConfiguration(IntPtr configuration)
        {
            return MidiSystem.Instance.EnqueueNativeCall(() =>
                CleanupConfiguration(configuration));
        }

        public static ApiType Api_GetApiType(MidiConfigurationHandle configuration)
        {
            return GetApiType(configuration);
        }

        public static bool Api_IsVirtualDeviceApiAvailable(MidiConfigurationHandle configuration)
        {
            return MidiSystem.Instance.EnqueueNativeCall(() =>
                IsVirtualDeviceApiAvailable(configuration));
        }

        public static bool Api_IsDevicesWatcherApiAvailable(MidiConfigurationHandle configuration)
        {
            return MidiSystem.Instance.EnqueueNativeCall(() =>
                IsDevicesWatcherApiAvailable(configuration));
        }

        public static bool Api_IsWmsInitialized(MidiConfigurationHandle configuration)
        {
            return MidiSystem.Instance.EnqueueNativeCall(() =>
                IsWmsInitialized(configuration));
        }

        public static void Api_CheckNativeApiActivityCallback(MidiConfigurationHandle configuration)
        {
            MidiSystem.Instance.EnqueueNativeCall(() =>
                CheckNativeApiActivityCallback(configuration));
        }

        public static void Api_CheckWinRtErrorHandling_Win(MidiConfigurationHandle configuration)
        {
            MidiSystem.Instance.EnqueueNativeCall(() =>
                CheckWinRtErrorHandling_Win(configuration));
        }

        public static void Api_CheckStdExceptionHandling_Win(MidiConfigurationHandle configuration)
        {
            MidiSystem.Instance.EnqueueNativeCall(() =>
                CheckStdExceptionHandling_Win(configuration));
        }

        #endregion
    }
}
