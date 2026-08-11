using System;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Common;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class MidiDevicesSessionApi
    {
        #region Nested enums

        public enum SESSION_CALLBACKOPERATION
        {
            SESSION_CALLBACKOPERATION_ENDPOINTADDED = 0,
            SESSION_CALLBACKOPERATION_ENDPOINTREMOVED = 1
        }

        public enum SESSION_OPENRESULT
        {
            SESSION_OPENRESULT_OK = 0,

            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            SESSION_OPENRESULT_CANTCREATEWMSSDKINITIALIZER = 1,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            SESSION_OPENRESULT_CANTINITIALIZEWMSSDK = 2,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            SESSION_OPENRESULT_OLDWMSSDK = 3,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            SESSION_OPENRESULT_WMSSERVICEUNAVAILABLE = 4,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            SESSION_OPENRESULT_WMSUNKNOWNERROR = 5,

            SESSION_OPENRESULT_SERVERSTARTERROR = 101,
            SESSION_OPENRESULT_WRONGTHREAD = 102,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            SESSION_OPENRESULT_NOTPERMITTED = 103,
            SESSION_OPENRESULT_UNKNOWNERROR = 104,
            SESSION_OPENRESULT_THREADSTARTERROR = 105,
            SESSION_OPENRESULT_CLIENTCREATIONTIMEOUT = 106,
        }

        public enum SESSION_CLOSERESULT
        {
            SESSION_CLOSERESULT_OK = 0,
            SESSION_CLOSERESULT_THREADEXITTIMEOUT = 101,
        }

        #endregion

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void InputEndpointCallback(IntPtr info, SESSION_CALLBACKOPERATION operation);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OutputEndpointCallback(IntPtr info, SESSION_CALLBACKOPERATION operation);

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial SESSION_OPENRESULT OpenSession_Win(
            string name,
            MidiConfigurationHandle configuration,
            InputEndpointCallback inputEndpointCallback,
            OutputEndpointCallback outputEndpointCallback,
            out IntPtr handle,
            out int errorCode);

        [LibraryImport(NativeApi.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial SESSION_OPENRESULT OpenSession_Mac(string name, MidiConfigurationHandle configuration, InputEndpointCallback inputEndpointCallback, OutputEndpointCallback outputEndpointCallback, out IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial SESSION_CLOSERESULT CloseSession(IntPtr handle);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern SESSION_OPENRESULT OpenSession_Win(
            string name,
            MidiConfigurationHandle configuration,
            InputEndpointCallback inputEndpointCallback,
            OutputEndpointCallback outputEndpointCallback,
            out IntPtr handle,
            out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern SESSION_OPENRESULT OpenSession_Mac(string name, MidiConfigurationHandle configuration, InputEndpointCallback inputEndpointCallback, OutputEndpointCallback outputEndpointCallback, out IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern SESSION_CLOSERESULT CloseSession(IntPtr handle);
#endif

        #endregion

        #region Methods

        public static SESSION_OPENRESULT Api_OpenSession(
            string name,
            MidiConfigurationHandle configuration,
            InputEndpointCallback inputEndpointCallback,
            OutputEndpointCallback outputEndpointCallback,
            out IntPtr handle,
            out int errorCode)
        {
            switch (CommonApi.Api_GetOsType())
            {
                case CommonApi.OS_TYPE.OS_TYPE_WIN:
                    return OpenSession_Win(
                        name,
                        configuration,
                        inputEndpointCallback,
                        outputEndpointCallback,
                        out handle,
                        out errorCode);
                case CommonApi.OS_TYPE.OS_TYPE_MAC:
                    return OpenSession_Mac(
                        name,
                        configuration,
                        inputEndpointCallback,
                        outputEndpointCallback,
                        out handle,
                        out errorCode);
            }

            throw new NotImplementedException();
        }

        public static SESSION_CLOSERESULT Api_CloseSession(IntPtr handle)
        {
            return CloseSession(handle);
        }

        #endregion
    }
}
