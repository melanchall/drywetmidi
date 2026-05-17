using System;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Common;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class OutputEndpointApi
    {
        #region Nested enums

        public enum OUT_GETCOUNTRESULT
        {
            OUT_GETCOUNTRESULT_OK = 0
        }

        public enum OUT_GETALLINFORESULT
        {
            OUT_GETALLINFORESULT_OK = 0,

            OUT_GETALLINFORESULT_BADDEVICEID = 1,
            OUT_GETALLINFORESULT_INVALIDSTRUCTURE = 2,
            OUT_GETALLINFORESULT_NODRIVER = 3,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            OUT_GETALLINFORESULT_NOMEMORY = 4,
            OUT_GETALLINFORESULT_UNKNOWNWMSERROR = 5,

            OUT_GETALLINFORESULT_UNKNOWNERROR = 1000,
            OUT_GETALLINFORESULT_UNKNOWNERRORONGETINFO = 1001
        }

        public enum OUT_OPENRESULT
        {
            OUT_OPENRESULT_OK = 0,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.InUse)]
            OUT_OPENRESULT_ALLOCATED = 1,
            OUT_OPENRESULT_BADDEVICEID = 2,
            OUT_OPENRESULT_INVALIDFLAG = 3,
            OUT_OPENRESULT_INVALIDSTRUCTURE = 4,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            OUT_OPENRESULT_NOMEMORY = 5,
            OUT_OPENRESULT_INVALIDCLIENT = 101,
            OUT_OPENRESULT_INVALIDPORT = 102,
            OUT_OPENRESULT_WRONGTHREAD = 103,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            OUT_OPENRESULT_NOTPERMITTED = 104,
            OUT_OPENRESULT_UNKNOWNERROR = 105
        }

        public enum OUT_CLOSERESULT
        {
            OUT_CLOSERESULT_OK = 0,
            OUT_CLOSERESULT_RESET_INVALIDHANDLE = 1,
            OUT_CLOSERESULT_RESET_UNKNOWNERROR = 1000,
            OUT_CLOSERESULT_CLOSE_STILLPLAYING = 2,
            OUT_CLOSERESULT_CLOSE_INVALIDHANDLE = 3,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            OUT_CLOSERESULT_CLOSE_NOMEMORY = 4,
            OUT_CLOSERESULT_CLOSE_UNKNOWNERROR = 2000
        }

        public enum OUT_SENDSHORTRESULT
        {
            OUT_SENDSHORTRESULT_OK = 0,
            OUT_SENDSHORTRESULT_BADOPENMODE = 1,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.Busy)]
            OUT_SENDSHORTRESULT_NOTREADY = 2,
            OUT_SENDSHORTRESULT_INVALIDHANDLE = 3,
            OUT_SENDSHORTRESULT_INVALIDCLIENT = 101,
            OUT_SENDSHORTRESULT_INVALIDPORT = 102,
            OUT_SENDSHORTRESULT_WRONGENDPOINT = 103,
            OUT_SENDSHORTRESULT_UNKNOWNENDPOINT = 104,
            OUT_SENDSHORTRESULT_COMMUNICATIONERROR = 105,
            OUT_SENDSHORTRESULT_SERVERSTARTERROR = 106,
            OUT_SENDSHORTRESULT_WRONGTHREAD = 107,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            OUT_SENDSHORTRESULT_NOTPERMITTED = 108,
            OUT_SENDSHORTRESULT_UNKNOWNERROR = 109
        }

        public enum OUT_SENDSYSEXRESULT
        {
            OUT_SENDSYSEXRESULT_OK = 0,
            OUT_SENDSYSEXRESULT_PREPAREBUFFER_INVALIDHANDLE = 1,
            OUT_SENDSYSEXRESULT_PREPAREBUFFER_INVALIDADDRESS = 2,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            OUT_SENDSYSEXRESULT_PREPAREBUFFER_NOMEMORY = 3,
            OUT_SENDSYSEXRESULT_PREPAREBUFFER_UNKNOWNERROR = 1000,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.Busy)]
            OUT_SENDSYSEXRESULT_NOTREADY = 4,
            OUT_SENDSYSEXRESULT_UNPREPARED = 5,
            OUT_SENDSYSEXRESULT_INVALIDHANDLE = 6,
            OUT_SENDSYSEXRESULT_INVALIDSTRUCTURE = 7,
            OUT_SENDSYSEXRESULT_INVALIDCLIENT = 101,
            OUT_SENDSYSEXRESULT_INVALIDPORT = 102,
            OUT_SENDSYSEXRESULT_WRONGENDPOINT = 103,
            OUT_SENDSYSEXRESULT_UNKNOWNENDPOINT = 104,
            OUT_SENDSYSEXRESULT_COMMUNICATIONERROR = 105,
            OUT_SENDSYSEXRESULT_SERVERSTARTERROR = 106,
            OUT_SENDSYSEXRESULT_WRONGTHREAD = 107,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            OUT_SENDSYSEXRESULT_NOTPERMITTED = 108,
            OUT_SENDSYSEXRESULT_UNKNOWNERROR = 109
        }

        public enum OUT_GETSYSEXDATARESULT
        {
            OUT_GETSYSEXDATARESULT_OK = 0,
            OUT_GETSYSEXDATARESULT_STILLPLAYING = 1,
            OUT_GETSYSEXDATARESULT_INVALIDSTRUCTURE = 2,
            OUT_GETSYSEXDATARESULT_INVALIDHANDLE = 3,
            OUT_GETSYSEXDATARESULT_UNKNOWNERROR = 1000
        }

        public enum OUT_GETPROPERTYRESULT
        {
            OUT_GETPROPERTYRESULT_OK = 0,
            OUT_GETPROPERTYRESULT_UNKNOWNENDPOINT = 101,
            OUT_GETPROPERTYRESULT_TOOLONG = 102,
            OUT_GETPROPERTYRESULT_UNKNOWNPROPERTY = 103,
            OUT_GETPROPERTYRESULT_UNKNOWNERROR = 104
        }

        #endregion

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Callback_Win(IntPtr hMidi, NativeApi.MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void CloneOutputEndpointInfo(IntPtr source, out IntPtr info);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETCOUNTRESULT GetOutputEndpointsCount(out int count);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETALLINFORESULT GetOutputEndpointsInfo(MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, out IntPtr devicesInfo, out int devicesCount, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void FreeOutputEndpointsInfo(IntPtr array, int size);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial int GetOutputEndpointHashCode(IntPtr info);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.U1)]
        private static partial bool AreOutputEndpointsEqual(IntPtr info1, IntPtr info2);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputEndpointName(IntPtr info, out IntPtr value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputEndpointManufacturer(IntPtr info, out IntPtr value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputEndpointProduct(IntPtr info, out IntPtr value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputEndpointDriverVersion(IntPtr info, out int value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_OPENRESULT OpenOutputEndpoint_Win(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Win callback, out IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_OPENRESULT OpenOutputEndpoint_Mac(IntPtr info, MidiDevicesSessionHandle sessionHandle, out IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_CLOSERESULT CloseOutputEndpoint(IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_SENDSHORTRESULT SendShortEventToOutputEndpoint(IntPtr handle, int message, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_SENDSYSEXRESULT SendSysExEventToOutputEndpoint_Mac(IntPtr handle, byte[] data, ushort dataSize, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_SENDSYSEXRESULT SendSysExEventToOutputEndpoint_Win(IntPtr handle, IntPtr data, int size, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETSYSEXDATARESULT GetOutputEndpointSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.U1)]
        private static partial bool IsOutputEndpointPropertySupported(OutputEndpointProperty property);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputEndpointTechnology(IntPtr info, out OutputEndpointTechnology value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputEndpointUniqueId(IntPtr info, out int value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputEndpointVoicesNumber(IntPtr info, out int value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputEndpointNotesNumber(IntPtr info, out int value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputEndpointChannelsMask(IntPtr info, out int value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputEndpointOptions(IntPtr info, out OutputEndpointOption value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputEndpointDriverOwner(IntPtr info, out IntPtr value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial void DeleteOutputEndpointInfo(IntPtr info);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CloneOutputEndpointInfo(IntPtr source, out IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETCOUNTRESULT GetOutputEndpointsCount(out int count);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETALLINFORESULT GetOutputEndpointsInfo(MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, out IntPtr devicesInfo, out int devicesCount, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeOutputEndpointsInfo(IntPtr array, int size);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetOutputEndpointHashCode(IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool AreOutputEndpointsEqual(IntPtr info1, IntPtr info2);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputEndpointName(IntPtr info, out IntPtr value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputEndpointManufacturer(IntPtr info, out IntPtr value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputEndpointProduct(IntPtr info, out IntPtr value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputEndpointDriverVersion(IntPtr info, out int value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_OPENRESULT OpenOutputEndpoint_Win(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Win callback, out IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_OPENRESULT OpenOutputEndpoint_Mac(IntPtr info, MidiDevicesSessionHandle sessionHandle, out IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_CLOSERESULT CloseOutputEndpoint(IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_SENDSHORTRESULT SendShortEventToOutputEndpoint(IntPtr handle, int message, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_SENDSYSEXRESULT SendSysExEventToOutputEndpoint_Mac(IntPtr handle, byte[] data, ushort dataSize, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_SENDSYSEXRESULT SendSysExEventToOutputEndpoint_Win(IntPtr handle, IntPtr data, int size, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETSYSEXDATARESULT GetOutputEndpointSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool IsOutputEndpointPropertySupported(OutputEndpointProperty property);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputEndpointTechnology(IntPtr info, out OutputEndpointTechnology value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputEndpointUniqueId(IntPtr info, out int value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputEndpointVoicesNumber(IntPtr info, out int value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputEndpointNotesNumber(IntPtr info, out int value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputEndpointChannelsMask(IntPtr info, out int value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputEndpointOptions(IntPtr info, out OutputEndpointOption value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputEndpointDriverOwner(IntPtr info, out IntPtr value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeleteOutputEndpointInfo(IntPtr info);
#endif

        #endregion

        #region Methods

        public static void Api_CloneOutputEndpointInfo(IntPtr source, out IntPtr info)
        {
            CloneOutputEndpointInfo(source, out info);
        }

        public static OUT_GETCOUNTRESULT Api_GetDevicesCount(out int count)
        {
            return GetOutputEndpointsCount(out count);
        }

        public static OUT_GETALLINFORESULT Api_GetDevicesInfo(MidiConfigurationHandle configuration, MidiDevicesSessionHandle sessionHandle, out IntPtr devicesInfo, out int devicesCount, out int errorCode)
        {
            return GetOutputEndpointsInfo(configuration, sessionHandle, out devicesInfo, out devicesCount, out errorCode);
        }

        public static void Api_FreeDevicesInfo(IntPtr array, int size)
        {
            FreeOutputEndpointsInfo(array, size);
        }

        public static int Api_GetDeviceHashCode(IntPtr info)
        {
            return GetOutputEndpointHashCode(info);
        }

        public static bool Api_AreDevicesEqual(IntPtr info1, IntPtr info2)
        {
            return AreOutputEndpointsEqual(info1, info2);
        }

        public static OUT_OPENRESULT Api_OpenDevice_Win(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Win callback, out IntPtr handle, out int errorCode)
        {
            return OpenOutputEndpoint_Win(info, sessionHandle, callback, out handle, out errorCode);
        }

        public static OUT_OPENRESULT Api_OpenDevice_Mac(IntPtr info, MidiDevicesSessionHandle sessionHandle, out IntPtr handle, out int errorCode)
        {
            return OpenOutputEndpoint_Mac(info, sessionHandle, out handle, out errorCode);
        }

        public static OUT_CLOSERESULT Api_CloseDevice(IntPtr handle, out int errorCode)
        {
            return CloseOutputEndpoint(handle, out errorCode);
        }

        public static OUT_SENDSHORTRESULT Api_SendShortEvent(IntPtr handle, int message, out int errorCode)
        {
            return SendShortEventToOutputEndpoint(handle, message, out errorCode);
        }

        public static OUT_SENDSYSEXRESULT Api_SendSysExEvent_Mac(IntPtr handle, byte[] data, ushort dataSize, out int errorCode)
        {
            return SendSysExEventToOutputEndpoint_Mac(handle, data, dataSize, out errorCode);
        }

        public static OUT_SENDSYSEXRESULT Api_SendSysExEvent_Win(IntPtr handle, IntPtr data, int size, out int errorCode)
        {
            return SendSysExEventToOutputEndpoint_Win(handle, data, size, out errorCode);
        }

        public static OUT_GETSYSEXDATARESULT Api_GetSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size, out int errorCode)
        {
            return GetOutputEndpointSysExBufferData(handle, header, out data, out size, out errorCode);
        }

        public static bool Api_IsPropertySupported(OutputEndpointProperty property)
        {
            return IsOutputEndpointPropertySupported(property);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceName(IntPtr info, out string name, out int errorCode)
        {
            IntPtr namePointer;
            var result = GetOutputEndpointName(info, out namePointer, out errorCode);
            name = NativeApi.GetStringFromPointer(namePointer);
            return result;
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceManufacturer(IntPtr info, out string manufacturer, out int errorCode)
        {
            IntPtr manufacturerPointer;
            var result = GetOutputEndpointManufacturer(info, out manufacturerPointer, out errorCode);
            manufacturer = NativeApi.GetStringFromPointer(manufacturerPointer);
            return result;
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceProduct(IntPtr info, out string product, out int errorCode)
        {
            IntPtr productPointer;
            var result = GetOutputEndpointProduct(info, out productPointer, out errorCode);
            product = NativeApi.GetStringFromPointer(productPointer);
            return result;
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceDriverVersion(IntPtr info, out int driverVersion, out int errorCode)
        {
            return GetOutputEndpointDriverVersion(info, out driverVersion, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceTechnology(IntPtr info, out OutputEndpointTechnology deviceType, out int errorCode)
        {
            return GetOutputEndpointTechnology(info, out deviceType, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceUniqueId(IntPtr info, out int uniqueId, out int errorCode)
        {
            return GetOutputEndpointUniqueId(info, out uniqueId, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceVoicesNumber(IntPtr info, out int voicesNumber, out int errorCode)
        {
            return GetOutputEndpointVoicesNumber(info, out voicesNumber, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceNotesNumber(IntPtr info, out int notesNumber, out int errorCode)
        {
            return GetOutputEndpointNotesNumber(info, out notesNumber, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceChannelsMask(IntPtr info, out int channelsMask, out int errorCode)
        {
            return GetOutputEndpointChannelsMask(info, out channelsMask, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceOptions(IntPtr info, out OutputEndpointOption option, out int errorCode)
        {
            return GetOutputEndpointOptions(info, out option, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceDriverOwner(IntPtr info, out string driverOwner, out int errorCode)
        {
            IntPtr driverOwnerPointer;
            var result = GetOutputEndpointDriverOwner(info, out driverOwnerPointer, out errorCode);
            driverOwner = NativeApi.GetStringFromPointer(driverOwnerPointer);
            return result;
        }

        public static void Api_DeleteDeviceInfo(IntPtr info)
        {
            DeleteOutputEndpointInfo(info);
        }

        #endregion
    }
}
