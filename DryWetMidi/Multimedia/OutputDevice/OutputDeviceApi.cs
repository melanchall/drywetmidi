using System;
using System.Runtime.InteropServices;

#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static partial class OutputDeviceApi
    {
        #region Nested enums

        public enum OUT_GETCOUNTRESULT
        {
            OUT_GETCOUNTRESULT_OK = 0
        }

        public enum OUT_GETINFORESULT
        {
            OUT_GETINFORESULT_OK = 0,
            OUT_GETINFORESULT_BADDEVICEID = 1,
            OUT_GETINFORESULT_INVALIDSTRUCTURE = 2,
            OUT_GETINFORESULT_NODRIVER = 3,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            OUT_GETINFORESULT_NOMEMORY = 4,
            OUT_GETINFORESULT_UNKNOWNERROR = 1000
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

        public delegate void Callback_Win(IntPtr hMidi, NativeApi.MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

        #endregion

        #region Extern functions

#if NET7_0_OR_GREATER
        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETCOUNTRESULT GetOutputDevicesCount(out int count);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETINFORESULT GetOutputDeviceInfo(int deviceIndex, out IntPtr info, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial int GetOutputDeviceHashCode(IntPtr info);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.U1)]
        private static partial bool AreOutputDevicesEqual(IntPtr info1, IntPtr info2);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputDeviceName(IntPtr info, out IntPtr value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputDeviceManufacturer(IntPtr info, out IntPtr value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputDeviceProduct(IntPtr info, out IntPtr value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputDeviceDriverVersion(IntPtr info, out int value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_OPENRESULT OpenOutputDevice_Win(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Win callback, out IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_OPENRESULT OpenOutputDevice_Mac(IntPtr info, MidiDevicesSessionHandle sessionHandle, out IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_CLOSERESULT CloseOutputDevice(IntPtr handle, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_SENDSHORTRESULT SendShortEventToOutputDevice(IntPtr handle, int message, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Mac(IntPtr handle, byte[] data, ushort dataSize, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Win(IntPtr handle, IntPtr data, int size, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETSYSEXDATARESULT GetOutputDeviceSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.U1)]
        private static partial bool IsOutputDevicePropertySupported(OutputDeviceProperty property);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputDeviceTechnology(IntPtr info, out OutputDeviceTechnology value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputDeviceUniqueId(IntPtr info, out int value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputDeviceVoicesNumber(IntPtr info, out int value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputDeviceNotesNumber(IntPtr info, out int value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputDeviceChannelsMask(IntPtr info, out int value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputDeviceOptions(IntPtr info, out OutputDeviceOption value, out int errorCode);

        [LibraryImport(NativeApi.LibraryName)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static partial OUT_GETPROPERTYRESULT GetOutputDeviceDriverOwner(IntPtr info, out IntPtr value, out int errorCode);
#else
        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETCOUNTRESULT GetOutputDevicesCount(out int count);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETINFORESULT GetOutputDeviceInfo(int deviceIndex, out IntPtr info, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetOutputDeviceHashCode(IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool AreOutputDevicesEqual(IntPtr info1, IntPtr info2);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceName(IntPtr info, out IntPtr value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceManufacturer(IntPtr info, out IntPtr value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceProduct(IntPtr info, out IntPtr value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceDriverVersion(IntPtr info, out int value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_OPENRESULT OpenOutputDevice_Win(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Win callback, out IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_OPENRESULT OpenOutputDevice_Mac(IntPtr info, MidiDevicesSessionHandle sessionHandle, out IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_CLOSERESULT CloseOutputDevice(IntPtr handle, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_SENDSHORTRESULT SendShortEventToOutputDevice(IntPtr handle, int message, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Mac(IntPtr handle, byte[] data, ushort dataSize, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Win(IntPtr handle, IntPtr data, int size, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETSYSEXDATARESULT GetOutputDeviceSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool IsOutputDevicePropertySupported(OutputDeviceProperty property);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceTechnology(IntPtr info, out OutputDeviceTechnology value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceUniqueId(IntPtr info, out int value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceVoicesNumber(IntPtr info, out int value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceNotesNumber(IntPtr info, out int value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceChannelsMask(IntPtr info, out int value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceOptions(IntPtr info, out OutputDeviceOption value, out int errorCode);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceDriverOwner(IntPtr info, out IntPtr value, out int errorCode);
#endif

        #endregion

        #region Methods

        public static OUT_GETCOUNTRESULT Api_GetDevicesCount(out int count)
        {
            return GetOutputDevicesCount(out count);
        }

        public static OUT_GETINFORESULT Api_GetDeviceInfo(int deviceIndex, out IntPtr info, out int errorCode)
        {
            return GetOutputDeviceInfo(deviceIndex, out info, out errorCode);
        }

        public static int Api_GetDeviceHashCode(IntPtr info)
        {
            return GetOutputDeviceHashCode(info);
        }

        public static bool Api_AreDevicesEqual(IntPtr info1, IntPtr info2)
        {
            return AreOutputDevicesEqual(info1, info2);
        }

        public static OUT_OPENRESULT Api_OpenDevice_Win(IntPtr info, MidiDevicesSessionHandle sessionHandle, Callback_Win callback, out IntPtr handle, out int errorCode)
        {
            return OpenOutputDevice_Win(info, sessionHandle, callback, out handle, out errorCode);
        }

        public static OUT_OPENRESULT Api_OpenDevice_Mac(IntPtr info, MidiDevicesSessionHandle sessionHandle, out IntPtr handle, out int errorCode)
        {
            return OpenOutputDevice_Mac(info, sessionHandle, out handle, out errorCode);
        }

        public static OUT_CLOSERESULT Api_CloseDevice(IntPtr handle, out int errorCode)
        {
            return CloseOutputDevice(handle, out errorCode);
        }

        public static OUT_SENDSHORTRESULT Api_SendShortEvent(IntPtr handle, int message, out int errorCode)
        {
            return SendShortEventToOutputDevice(handle, message, out errorCode);
        }

        public static OUT_SENDSYSEXRESULT Api_SendSysExEvent_Mac(IntPtr handle, byte[] data, ushort dataSize, out int errorCode)
        {
            return SendSysExEventToOutputDevice_Mac(handle, data, dataSize, out errorCode);
        }

        public static OUT_SENDSYSEXRESULT Api_SendSysExEvent_Win(IntPtr handle, IntPtr data, int size, out int errorCode)
        {
            return SendSysExEventToOutputDevice_Win(handle, data, size, out errorCode);
        }

        public static OUT_GETSYSEXDATARESULT Api_GetSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size, out int errorCode)
        {
            return GetOutputDeviceSysExBufferData(handle, header, out data, out size, out errorCode);
        }

        public static bool Api_IsPropertySupported(OutputDeviceProperty property)
        {
            return IsOutputDevicePropertySupported(property);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceName(IntPtr info, out string name, out int errorCode)
        {
            IntPtr namePointer;
            var result = GetOutputDeviceName(info, out namePointer, out errorCode);
            name = NativeApi.GetStringFromPointer(namePointer);
            return result;
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceManufacturer(IntPtr info, out string manufacturer, out int errorCode)
        {
            IntPtr manufacturerPointer;
            var result = GetOutputDeviceManufacturer(info, out manufacturerPointer, out errorCode);
            manufacturer = NativeApi.GetStringFromPointer(manufacturerPointer);
            return result;
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceProduct(IntPtr info, out string product, out int errorCode)
        {
            IntPtr productPointer;
            var result = GetOutputDeviceProduct(info, out productPointer, out errorCode);
            product = NativeApi.GetStringFromPointer(productPointer);
            return result;
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceDriverVersion(IntPtr info, out int driverVersion, out int errorCode)
        {
            return GetOutputDeviceDriverVersion(info, out driverVersion, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceTechnology(IntPtr info, out OutputDeviceTechnology deviceType, out int errorCode)
        {
            return GetOutputDeviceTechnology(info, out deviceType, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceUniqueId(IntPtr info, out int uniqueId, out int errorCode)
        {
            return GetOutputDeviceUniqueId(info, out uniqueId, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceVoicesNumber(IntPtr info, out int voicesNumber, out int errorCode)
        {
            return GetOutputDeviceVoicesNumber(info, out voicesNumber, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceNotesNumber(IntPtr info, out int notesNumber, out int errorCode)
        {
            return GetOutputDeviceNotesNumber(info, out notesNumber, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceChannelsMask(IntPtr info, out int channelsMask, out int errorCode)
        {
            return GetOutputDeviceChannelsMask(info, out channelsMask, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceOptions(IntPtr info, out OutputDeviceOption option, out int errorCode)
        {
            return GetOutputDeviceOptions(info, out option, out errorCode);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceDriverOwner(IntPtr info, out string driverOwner, out int errorCode)
        {
            IntPtr driverOwnerPointer;
            var result = GetOutputDeviceDriverOwner(info, out driverOwnerPointer, out errorCode);
            driverOwner = NativeApi.GetStringFromPointer(driverOwnerPointer);
            return result;
        }

        #endregion
    }
}
