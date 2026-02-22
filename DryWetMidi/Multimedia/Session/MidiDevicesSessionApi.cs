using System;
using System.Runtime.InteropServices;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class MidiDevicesSessionApi
    {
        #region Nested enums

        public enum SESSION_OPENRESULT
        {
            SESSION_OPENRESULT_OK = 0,
            SESSION_OPENRESULT_SERVERSTARTERROR = 101,
            SESSION_OPENRESULT_WRONGTHREAD = 102,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            SESSION_OPENRESULT_NOTPERMITTED = 103,
            SESSION_OPENRESULT_UNKNOWNERROR = 104,
            SESSION_OPENRESULT_THREADSTARTERROR = 105,
        }

        public enum SESSION_CLOSERESULT
        {
            SESSION_CLOSERESULT_OK = 0,
            SESSION_CLOSERESULT_THREADEXITTIMEOUT = 101,
        }

        #endregion

        #region Delegates

        public delegate void InputDeviceCallback(IntPtr info, bool operation);
        public delegate void OutputDeviceCallback(IntPtr info, bool operation);

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial SESSION_OPENRESULT OpenSession_Mac(IntPtr name, InputDeviceCallback inputDeviceCallback, OutputDeviceCallback outputDeviceCallback, out IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial SESSION_OPENRESULT OpenSession_Win(IntPtr name, out IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial SESSION_CLOSERESULT CloseSession(IntPtr handle);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern SESSION_OPENRESULT OpenSession_Mac(IntPtr name, InputDeviceCallback inputDeviceCallback, OutputDeviceCallback outputDeviceCallback, out IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern SESSION_OPENRESULT OpenSession_Win(IntPtr name, out IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern SESSION_CLOSERESULT CloseSession(IntPtr handle);
#endif

        #endregion

        #region Methods

        public static SESSION_OPENRESULT Api_OpenSession_Mac(
            IntPtr name,
            InputDeviceCallback inputDeviceCallback,
            OutputDeviceCallback outputDeviceCallback,
            out IntPtr handle,
            out int errorCode)
        {
            return OpenSession_Mac(name, inputDeviceCallback, outputDeviceCallback, out handle, out errorCode);
        }

        public static SESSION_OPENRESULT Api_OpenSession_Win(
            IntPtr name,
            out IntPtr handle,
            out int errorCode)
        {
            return OpenSession_Win(name, out handle, out errorCode);
        }

        public static SESSION_CLOSERESULT Api_CloseSession(IntPtr handle)
        {
            return CloseSession(handle);
        }

        #endregion
    }
}
