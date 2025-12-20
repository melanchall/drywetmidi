using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class OutputDeviceApi
    {
        #region Nested enums

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

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int GetOutputDevicesCount();

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETINFORESULT GetOutputDeviceInfo(int deviceIndex, out IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int GetOutputDeviceHashCode(IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool AreOutputDevicesEqual(IntPtr info1, IntPtr info2);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceName(IntPtr info, out IntPtr value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceManufacturer(IntPtr info, out IntPtr value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceProduct(IntPtr info, out IntPtr value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceDriverVersion(IntPtr info, out int value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_OPENRESULT OpenOutputDevice_Win(IntPtr info, IntPtr sessionHandle, Callback_Win callback, out IntPtr handle);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_OPENRESULT OpenOutputDevice_Mac(IntPtr info, IntPtr sessionHandle, out IntPtr handle);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_CLOSERESULT CloseOutputDevice(IntPtr handle);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_SENDSHORTRESULT SendShortEventToOutputDevice(IntPtr handle, int message);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Mac(IntPtr handle, byte[] data, ushort dataSize);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Win(IntPtr handle, IntPtr data, int size);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETSYSEXDATARESULT GetOutputDeviceSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool IsOutputDevicePropertySupported(OutputDeviceProperty property);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceTechnology(IntPtr info, out OutputDeviceTechnology value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceUniqueId(IntPtr info, out int value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceVoicesNumber(IntPtr info, out int value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceNotesNumber(IntPtr info, out int value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceChannelsMask(IntPtr info, out int value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceOptions(IntPtr info, out OutputDeviceOption value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern OUT_GETPROPERTYRESULT GetOutputDeviceDriverOwner(IntPtr info, out IntPtr value);

        #endregion

        #region Methods

        public static int Api_GetDevicesCount()
        {
            return GetOutputDevicesCount();
        }

        public static OUT_GETINFORESULT Api_GetDeviceInfo(int deviceIndex, out IntPtr info)
        {
            return GetOutputDeviceInfo(deviceIndex, out info);
        }

        public static int Api_GetDeviceHashCode(IntPtr info)
        {
            return GetOutputDeviceHashCode(info);
        }

        public static bool Api_AreDevicesEqual(IntPtr info1, IntPtr info2)
        {
            return AreOutputDevicesEqual(info1, info2);
        }

        public static OUT_OPENRESULT Api_OpenDevice_Win(IntPtr info, IntPtr sessionHandle, Callback_Win callback, out IntPtr handle)
        {
            return OpenOutputDevice_Win(info, sessionHandle, callback, out handle);
        }

        public static OUT_OPENRESULT Api_OpenDevice_Mac(IntPtr info, IntPtr sessionHandle, out IntPtr handle)
        {
            return OpenOutputDevice_Mac(info, sessionHandle, out handle);
        }

        public static OUT_CLOSERESULT Api_CloseDevice(IntPtr handle)
        {
            return CloseOutputDevice(handle);
        }

        public static OUT_SENDSHORTRESULT Api_SendShortEvent(IntPtr handle, int message)
        {
            return SendShortEventToOutputDevice(handle, message);
        }

        public static OUT_SENDSYSEXRESULT Api_SendSysExEvent_Mac(IntPtr handle, byte[] data, ushort dataSize)
        {
            return SendSysExEventToOutputDevice_Mac(handle, data, dataSize);
        }

        public static OUT_SENDSYSEXRESULT Api_SendSysExEvent_Win(IntPtr handle, IntPtr data, int size)
        {
            return SendSysExEventToOutputDevice_Win(handle, data, size);
        }

        public static OUT_GETSYSEXDATARESULT Api_GetSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size)
        {
            return GetOutputDeviceSysExBufferData(handle, header, out data, out size);
        }

        public static bool Api_IsPropertySupported(OutputDeviceProperty property)
        {
            return IsOutputDevicePropertySupported(property);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceName(IntPtr info, out string name)
        {
            IntPtr namePointer;
            var result = GetOutputDeviceName(info, out namePointer);
            name = NativeApi.GetStringFromPointer(namePointer);
            return result;
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceManufacturer(IntPtr info, out string manufacturer)
        {
            IntPtr manufacturerPointer;
            var result = GetOutputDeviceManufacturer(info, out manufacturerPointer);
            manufacturer = NativeApi.GetStringFromPointer(manufacturerPointer);
            return result;
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceProduct(IntPtr info, out string product)
        {
            IntPtr productPointer;
            var result = GetOutputDeviceProduct(info, out productPointer);
            product = NativeApi.GetStringFromPointer(productPointer);
            return result;
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceDriverVersion(IntPtr info, out int driverVersion)
        {
            return GetOutputDeviceDriverVersion(info, out driverVersion);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceTechnology(IntPtr info, out OutputDeviceTechnology deviceType)
        {
            return GetOutputDeviceTechnology(info, out deviceType);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceUniqueId(IntPtr info, out int uniqueId)
        {
            return GetOutputDeviceUniqueId(info, out uniqueId);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceVoicesNumber(IntPtr info, out int voicesNumber)
        {
            return GetOutputDeviceVoicesNumber(info, out voicesNumber);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceNotesNumber(IntPtr info, out int notesNumber)
        {
            return GetOutputDeviceNotesNumber(info, out notesNumber);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceChannelsMask(IntPtr info, out int channelsMask)
        {
            return GetOutputDeviceChannelsMask(info, out channelsMask);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceOptions(IntPtr info, out OutputDeviceOption option)
        {
            return GetOutputDeviceOptions(info, out option);
        }

        public static OUT_GETPROPERTYRESULT Api_GetDeviceDriverOwner(IntPtr info, out string driverOwner)
        {
            IntPtr driverOwnerPointer;
            var result = GetOutputDeviceDriverOwner(info, out driverOwnerPointer);
            driverOwner = NativeApi.GetStringFromPointer(driverOwnerPointer);
            return result;
        }

        #endregion
    }
}
