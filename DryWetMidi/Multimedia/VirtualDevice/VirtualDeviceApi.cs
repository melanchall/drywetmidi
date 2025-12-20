using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class VirtualDeviceApi
    {
        #region Nested enums

        public enum VIRTUAL_OPENRESULT
        {
            VIRTUAL_OPENRESULT_OK = 0,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            VIRTUAL_OPENRESULT_CREATESOURCE_NOTPERMITTED = 101,
            VIRTUAL_OPENRESULT_CREATESOURCE_SERVERSTARTERROR = 102,
            VIRTUAL_OPENRESULT_CREATESOURCE_WRONGTHREAD = 103,
            VIRTUAL_OPENRESULT_CREATESOURCE_UNKNOWNERROR = 104,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            VIRTUAL_OPENRESULT_CREATEDESTINATION_NOTPERMITTED = 105,
            VIRTUAL_OPENRESULT_CREATEDESTINATION_SERVERSTARTERROR = 106,
            VIRTUAL_OPENRESULT_CREATEDESTINATION_WRONGTHREAD = 107,
            VIRTUAL_OPENRESULT_CREATEDESTINATION_UNKNOWNERROR = 108
        }

        public enum VIRTUAL_CLOSERESULT
        {
            VIRTUAL_CLOSERESULT_OK = 0,
            VIRTUAL_CLOSERESULT_DISPOSESOURCE_UNKNOWNENDPOINT = 101,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            VIRTUAL_CLOSERESULT_DISPOSESOURCE_NOTPERMITTED = 102,
            VIRTUAL_CLOSERESULT_DISPOSESOURCE_UNKNOWNERROR = 103,
            VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_UNKNOWNENDPOINT = 104,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_NOTPERMITTED = 105,
            VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_UNKNOWNERROR = 106
        }

        public enum VIRTUAL_SENDBACKRESULT
        {
            VIRTUAL_SENDBACKRESULT_OK = 0,
            VIRTUAL_SENDBACKRESULT_UNKNOWNERROR_TE = 1,
            VIRTUAL_SENDBACKRESULT_UNKNOWNENDPOINT = 101,
            VIRTUAL_SENDBACKRESULT_WRONGENDPOINT = 102,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            VIRTUAL_SENDBACKRESULT_NOTPERMITTED = 103,
            VIRTUAL_SENDBACKRESULT_SERVERSTARTERROR = 104,
            VIRTUAL_SENDBACKRESULT_WRONGTHREAD = 105,
            VIRTUAL_SENDBACKRESULT_UNKNOWNERROR = 106,
            VIRTUAL_SENDBACKRESULT_MESSAGESENDERROR = 107
        }

        #endregion

        #region Delegates

        public delegate void Callback_Mac(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon);

        #endregion

        #region Extern functions

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern VIRTUAL_OPENRESULT OpenVirtualDevice_Mac(IntPtr name, IntPtr sessionHandle, Callback_Mac callback, out IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern VIRTUAL_CLOSERESULT CloseVirtualDevice(IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern VIRTUAL_SENDBACKRESULT SendDataBackFromVirtualDevice(IntPtr pktlist, IntPtr readProcRefCon);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetInputDeviceInfoFromVirtualDevice(IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetOutputDeviceInfoFromVirtualDevice(IntPtr info);

        #endregion

        #region Methods

        public static VIRTUAL_OPENRESULT Api_OpenDevice_Mac(string name, IntPtr sessionHandle, Callback_Mac callback, out IntPtr info)
        {
            var namePointer = Marshal.StringToHGlobalAnsi(name);
            return OpenVirtualDevice_Mac(namePointer, sessionHandle, callback, out info);
        }

        public static VIRTUAL_CLOSERESULT Api_CloseDevice(IntPtr info)
        {
            return CloseVirtualDevice(info);
        }

        public static VIRTUAL_SENDBACKRESULT Api_SendDataBack(IntPtr pktlist, IntPtr readProcRefCon)
        {
            return SendDataBackFromVirtualDevice(pktlist, readProcRefCon);
        }

        public static IntPtr Api_GetInputDeviceInfo(IntPtr info)
        {
            return GetInputDeviceInfoFromVirtualDevice(info);
        }

        public static IntPtr Api_GetOutputDeviceInfo(IntPtr info)
        {
            return GetOutputDeviceInfoFromVirtualDevice(info);
        }

        #endregion
    }
}
