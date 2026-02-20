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

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial API_TYPE GetApiType();

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.U1)]
        private static partial bool CanCompareDevices();
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern API_TYPE GetApiType();

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.U1)]
        private static extern bool CanCompareDevices();
#endif

        #endregion

        #region Methods

        public static API_TYPE Api_GetApiType()
        {
            return GetApiType();
        }

        public static bool Api_CanCompareDevices()
        {
            return CanCompareDevices();
        }

        #endregion
    }
}
