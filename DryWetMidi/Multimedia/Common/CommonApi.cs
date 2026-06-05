using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.InteropServices;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class CommonApi
    {
        #region Nested enums

        public enum API_TYPE
        {
            API_TYPE_WIN = 0,
            API_TYPE_MAC = 1
        }

        public enum WMSSERVICECHECKRESULT
        {
            WMSSERVICECHECKRESULT_OK = 0,
            WMSSERVICECHECKRESULT_ERROR_OPENSCMANAGER = 1,
            WMSSERVICECHECKRESULT_ERROR_OPENSERVICE = 2,
            WMSSERVICECHECKRESULT_ERROR_QUERYSERVICECONFIG_1 = 3,
            WMSSERVICECHECKRESULT_ERROR_ALLOCSERVICECONFIG = 4,
            WMSSERVICECHECKRESULT_ERROR_QUERYSERVICECONFIG_2 = 5,
            WMSSERVICECHECKRESULT_ERROR_SERVICEDISABLED = 6,
        }

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial API_TYPE GetApiType();

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void GetNativeEnvironmentInfo_Win(
            [MarshalAs(UnmanagedType.U1)] out bool comInitializationResult,
            [MarshalAs(UnmanagedType.U1)] out bool registryCheckResult,
            [MarshalAs(UnmanagedType.U1)] out bool comCheckResult,
            out WMSSERVICECHECKRESULT serviceCheckResult,
            [MarshalAs(UnmanagedType.U1)] out bool sdkCheckResult);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void FreeBuffer(IntPtr buffer);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern API_TYPE GetApiType();

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetNativeEnvironmentInfo_Win(
            [MarshalAs(UnmanagedType.U1)] out bool comInitializationResult,
            [MarshalAs(UnmanagedType.U1)] out bool registryCheckResult,
            [MarshalAs(UnmanagedType.U1)] out bool comCheckResult,
            out WMSSERVICECHECKRESULT serviceCheckResult,
            [MarshalAs(UnmanagedType.U1)] out bool sdkCheckResult);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeBuffer(IntPtr buffer);
#endif

        #endregion

        #region Methods

        public static API_TYPE Api_GetApiType()
        {
            return GetApiType();
        }

        public static void Api_GetNativeEnvironmentInfo_Win(
            out bool comInitializationResult,
            out bool registryCheckResult,
            out bool comCheckResult,
            out WMSSERVICECHECKRESULT serviceCheckResult,
            out bool sdkCheckResult)
        {
            GetNativeEnvironmentInfo_Win(
                out comInitializationResult,
                out registryCheckResult,
                out comCheckResult,
                out serviceCheckResult,
                out sdkCheckResult);
        }

        public static void Api_FreeBuffer(IntPtr buffer)
        {
            FreeBuffer(buffer);
        }

        #endregion
    }
}
