using System;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Common;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class VirtualDeviceApi
    {
        #region Nested enums

        public enum VIRTUAL_OPENRESULT
        {
            VIRTUAL_OPENRESULT_OK = 0,

            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            VIRTUAL_OPENRESULT_WMSUNAVAILABLE = 1,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            VIRTUAL_OPENRESULT_WMSBASICLOOPBACKUNAVAILABLE = 2,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            VIRTUAL_OPENRESULT_FAILED = 3,
            VIRTUAL_OPENRESULT_FAILEDGETINPUTDEVICEINFO = 4,
            VIRTUAL_OPENRESULT_FAILEDGETOUTPUTDEVICEINFO = 5,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            VIRTUAL_OPENRESULT_WMSERROR = 6,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            VIRTUAL_OPENRESULT_INVALIDNAME = 7,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            VIRTUAL_OPENRESULT_INVALIDUNIQUEID = 8,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            VIRTUAL_OPENRESULT_NAMEINUSE = 9,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.WmsError)]
            VIRTUAL_OPENRESULT_UNIQUEIDINUSE = 10,

            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            VIRTUAL_OPENRESULT_CREATESOURCE_NOTPERMITTED = 101,
            VIRTUAL_OPENRESULT_CREATESOURCE_SERVERSTARTERROR = 102,
            VIRTUAL_OPENRESULT_CREATESOURCE_WRONGTHREAD = 103,
            VIRTUAL_OPENRESULT_CREATESOURCE_UNKNOWNERROR = 104,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            VIRTUAL_OPENRESULT_CREATEDESTINATION_NOTPERMITTED = 105,
            VIRTUAL_OPENRESULT_CREATEDESTINATION_SERVERSTARTERROR = 106,
            VIRTUAL_OPENRESULT_CREATEDESTINATION_WRONGTHREAD = 107,
            VIRTUAL_OPENRESULT_CREATEDESTINATION_UNKNOWNERROR = 108,
            VIRTUAL_OPENRESULT_CREATESOURCE_FAILEDPROCESSNAME = 109,
            VIRTUAL_OPENRESULT_CREATEDESTINATION_FAILEDPROCESSNAME = 110
        }

        public enum VIRTUAL_CLOSERESULT
        {
            VIRTUAL_CLOSERESULT_OK = 0,

            VIRTUAL_CLOSERESULT_FAILED = 1,
            VIRTUAL_CLOSERESULT_WMSERROR = 2,

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

        public enum VIRTUAL_MUTERESULT
        {
            VIRTUAL_MUTERESULT_OK = 0,

            VIRTUAL_MUTERESULT_FAILED = 1,
            VIRTUAL_MUTERESULT_WMSERROR = 2,
        }

        public enum VIRTUAL_UNMUTERESULT
        {
            VIRTUAL_UNMUTERESULT_OK = 0,

            VIRTUAL_UNMUTERESULT_FAILED = 1,
            VIRTUAL_UNMUTERESULT_WMSERROR = 2,
        }

        #endregion

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Callback_Mac(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon);

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial VIRTUAL_OPENRESULT OpenVirtualDevice_Mac(string name, MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, Callback_Mac callback, out IntPtr info, out int errorCode);

        [LibraryImport(NativeApi.LibraryName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial VIRTUAL_OPENRESULT OpenVirtualDevice_Win(string name, MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, out IntPtr info, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial VIRTUAL_CLOSERESULT CloseVirtualDevice(IntPtr info, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial VIRTUAL_SENDBACKRESULT SendDataBackFromVirtualDevice(IntPtr pktlist, IntPtr readProcRefCon, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetInputDeviceInfoFromVirtualDevice(IntPtr info);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial IntPtr GetOutputDeviceInfoFromVirtualDevice(IntPtr info);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial VIRTUAL_MUTERESULT MuteVirtualDevice(IntPtr info, MidiConfigurationHandle configuration);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial VIRTUAL_UNMUTERESULT UnmuteVirtualDevice(IntPtr info, MidiConfigurationHandle configuration);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern VIRTUAL_OPENRESULT OpenVirtualDevice_Mac(string name, MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, Callback_Mac callback, out IntPtr info, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern VIRTUAL_OPENRESULT OpenVirtualDevice_Win(string name, MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, out IntPtr info, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern VIRTUAL_CLOSERESULT CloseVirtualDevice(IntPtr info, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern VIRTUAL_SENDBACKRESULT SendDataBackFromVirtualDevice(IntPtr pktlist, IntPtr readProcRefCon, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetInputDeviceInfoFromVirtualDevice(IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetOutputDeviceInfoFromVirtualDevice(IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern VIRTUAL_MUTERESULT MuteVirtualDevice(IntPtr info, MidiConfigurationHandle configuration);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern VIRTUAL_UNMUTERESULT UnmuteVirtualDevice(IntPtr info, MidiConfigurationHandle configuration);
#endif

        #endregion

        #region Methods

        public static VIRTUAL_OPENRESULT Api_OpenDevice_Mac(string name, MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, Callback_Mac callback, out IntPtr info, out int errorCode)
        {
            return OpenVirtualDevice_Mac(name, configuration, sessionHandle, callback, out info, out errorCode);
        }

        public static VIRTUAL_OPENRESULT Api_OpenDevice_Win(string name, MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, out IntPtr info, out int errorCode)
        {
            return OpenVirtualDevice_Win(name, configuration, sessionHandle, out info, out errorCode);
        }

        public static VIRTUAL_CLOSERESULT Api_CloseDevice(IntPtr info, out int errorCode)
        {
            return CloseVirtualDevice(info, out errorCode);
        }

        public static VIRTUAL_SENDBACKRESULT Api_SendDataBack(IntPtr pktlist, IntPtr readProcRefCon, out int errorCode)
        {
            return SendDataBackFromVirtualDevice(pktlist, readProcRefCon, out errorCode);
        }

        public static IntPtr Api_GetInputDeviceInfo(IntPtr info)
        {
            return GetInputDeviceInfoFromVirtualDevice(info);
        }

        public static IntPtr Api_GetOutputDeviceInfo(IntPtr info)
        {
            return GetOutputDeviceInfoFromVirtualDevice(info);
        }

        public static VIRTUAL_MUTERESULT Api_MuteDevice(IntPtr info, MidiConfigurationHandle configuration)
        {
            return MuteVirtualDevice(info, configuration);
        }

        public static VIRTUAL_UNMUTERESULT Api_UnmuteDevice(IntPtr info, MidiConfigurationHandle configuration)
        {
            return UnmuteVirtualDevice(info, configuration);
        }

        #endregion
    }
}
