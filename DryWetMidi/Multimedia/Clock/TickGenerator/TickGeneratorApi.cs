using System;
using System.Runtime.InteropServices;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class TickGeneratorApi
    {
        #region Nested enums

        public enum TG_STARTRESULT
        {
            TG_STARTRESULT_OK = 0,

            TG_STARTRESULT_CANTGETDEVICECAPABILITIES = 1,
            TG_STARTRESULT_CANTSETTIMERCALLBACK = 2,

            TG_STARTRESULT_NORESOURCES = 101,
            TG_STARTRESULT_BADTHREADATTRIBUTE = 102,
            TG_STARTRESULT_UNKNOWNERROR = 199
        }

        public enum TG_STOPRESULT
        {
            TG_STOPRESULT_OK = 0,

            TG_STOPRESULT_CANTENDPERIOD = 1,
            TG_STOPRESULT_CANTKILLEVENT = 2
        }

        #endregion

        #region Delegates

        public delegate void TimerCallback_Win(uint uID, uint uMsg, uint dwUser, uint dw1, uint dw2);

        public delegate void TimerCallback_Mac();

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial TG_STARTRESULT StartHighPrecisionTickGenerator_Win(int interval, IntPtr sessionHandle, TimerCallback_Win callback, out IntPtr info, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial TG_STARTRESULT StartHighPrecisionTickGenerator_Mac(int interval, IntPtr sessionHandle, TimerCallback_Mac callback, out IntPtr info, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial TG_STOPRESULT StopHighPrecisionTickGenerator(IntPtr sessionHandle, IntPtr info, out int errorCode);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern TG_STARTRESULT StartHighPrecisionTickGenerator_Win(int interval, IntPtr sessionHandle, TimerCallback_Win callback, out IntPtr info, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern TG_STARTRESULT StartHighPrecisionTickGenerator_Mac(int interval, IntPtr sessionHandle, TimerCallback_Mac callback, out IntPtr info, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern TG_STOPRESULT StopHighPrecisionTickGenerator(IntPtr sessionHandle, IntPtr info, out int errorCode);
#endif

        #endregion

        #region Methods

        public static TG_STARTRESULT Api_StartHighPrecisionTickGenerator_Win(int interval, IntPtr sessionHandle, TimerCallback_Win callback, out IntPtr info, out int errorCode)
        {
            return StartHighPrecisionTickGenerator_Win(interval, sessionHandle, callback, out info, out errorCode);
        }

        public static TG_STARTRESULT Api_StartHighPrecisionTickGenerator_Mac(int interval, IntPtr sessionHandle, TimerCallback_Mac callback, out IntPtr info, out int errorCode)
        {
            return StartHighPrecisionTickGenerator_Mac(interval, sessionHandle, callback, out info, out errorCode);
        }

        public static TG_STOPRESULT Api_StopHighPrecisionTickGenerator(IntPtr sessionHandle, IntPtr info, out int errorCode)
        {
            return StopHighPrecisionTickGenerator(sessionHandle, info, out errorCode);
        }

        #endregion
    }
}
