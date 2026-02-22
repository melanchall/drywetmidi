using System;
using System.Runtime.InteropServices;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class TickGeneratorSessionApi
    {
        #region Nested enums

        public enum TGSESSION_OPENRESULT
        {
            TGSESSION_OPENRESULT_OK = 0,

            TGSESSION_OPENRESULT_FAILEDTOGETTIMEBASEINFO = 101,
            TGSESSION_OPENRESULT_FAILEDTOSETREALTIMEPRIORITY = 102,
            TGSESSION_OPENRESULT_THREADSTARTERROR = 103
        }

        public enum TGSESSION_CLOSERESULT
        {
            TGSESSION_CLOSERESULT_OK = 0,

            TGSESSION_CLOSERESULT_THREADEXITTIMEOUT = 101,
        }

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial TGSESSION_OPENRESULT OpenTickGeneratorSession(out IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial TGSESSION_CLOSERESULT CloseTickGeneratorSession(IntPtr handle, out int errorCode);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern TGSESSION_OPENRESULT OpenTickGeneratorSession(out IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern TGSESSION_CLOSERESULT CloseTickGeneratorSession(IntPtr handle, out int errorCode);
#endif

        #endregion

        #region Methods

        public static TGSESSION_OPENRESULT Api_OpenSession(out IntPtr handle, out int errorCode)
        {
            return OpenTickGeneratorSession(out handle, out errorCode);
        }

        public static TGSESSION_CLOSERESULT Api_CloseSession(IntPtr handle, out int errorCode)
        {
            return CloseTickGeneratorSession(handle, out errorCode);
        }

        #endregion
    }
}
