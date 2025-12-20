using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class MidiDevicesSessionApi
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
            SESSION_CLOSERESULT_OK = 0
        }

        #endregion

        #region Delegates

        public delegate void InputDeviceCallback(IntPtr info, bool operation);
        public delegate void OutputDeviceCallback(IntPtr info, bool operation);

        #endregion

        #region Extern functions

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern SESSION_OPENRESULT OpenSession_Mac(IntPtr name, InputDeviceCallback inputDeviceCallback, OutputDeviceCallback outputDeviceCallback, out IntPtr handle);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern SESSION_OPENRESULT OpenSession_Win(IntPtr name, out IntPtr handle);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern SESSION_CLOSERESULT CloseSession(IntPtr handle);

        #endregion

        #region Methods

        public static SESSION_OPENRESULT Api_OpenSession_Mac(
            IntPtr name,
            InputDeviceCallback inputDeviceCallback,
            OutputDeviceCallback outputDeviceCallback,
            out IntPtr handle)
        {
            return OpenSession_Mac(name, inputDeviceCallback, outputDeviceCallback, out handle);
        }

        public static SESSION_OPENRESULT Api_OpenSession_Win(IntPtr name, out IntPtr handle)
        {
            return OpenSession_Win(name, out handle);
        }

        public static SESSION_CLOSERESULT Api_CloseSession(IntPtr handle)
        {
            return CloseSession(handle);
        }

        #endregion
    }
}
