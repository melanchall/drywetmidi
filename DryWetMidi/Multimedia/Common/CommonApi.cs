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

        public enum OS_TYPE
        {
            OS_TYPE_WIN = 0,
            OS_TYPE_MAC = 1
        }

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OS_TYPE GetOsType();

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void GetNativeEnvironmentInfo_Win(
            [MarshalAs(UnmanagedType.U1)] out bool wmsAvailable);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void FreeBuffer(IntPtr buffer);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OS_TYPE GetOsType();

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetNativeEnvironmentInfo_Win(
            [MarshalAs(UnmanagedType.U1)] out bool wmsAvailable);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeBuffer(IntPtr buffer);
#endif

        #endregion

        #region Methods

        public static OS_TYPE Api_GetOsType()
        {
            return GetOsType();
        }

        public static void Api_GetNativeEnvironmentInfo_Win(
            out bool wmsAvailable)
        {
            GetNativeEnvironmentInfo_Win(
                out wmsAvailable);
        }

        public static void Api_FreeBuffer(IntPtr buffer)
        {
            FreeBuffer(buffer);
        }

        #endregion
    }
}
