using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class InputDeviceApi
    {
        #region Nested enums

        public enum IN_GETINFORESULT
        {
            IN_GETINFORESULT_OK = 0,

            IN_GETINFORESULT_BADDEVICEID = 1,
            IN_GETINFORESULT_INVALIDSTRUCTURE = 2,
            IN_GETINFORESULT_NODRIVER = 3,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            IN_GETINFORESULT_NOMEMORY = 4,

            IN_GETINFORESULT_UNKNOWNERROR = 1000
        }

        public enum IN_OPENRESULT
        {
            IN_OPENRESULT_OK = 0,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.InUse)]
            IN_OPENRESULT_ALLOCATED = 1,
            IN_OPENRESULT_BADDEVICEID = 2,
            IN_OPENRESULT_INVALIDFLAG = 3,
            IN_OPENRESULT_INVALIDSTRUCTURE = 4,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            IN_OPENRESULT_NOMEMORY = 5,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            IN_OPENRESULT_PREPAREBUFFER_NOMEMORY = 6,
            IN_OPENRESULT_PREPAREBUFFER_INVALIDHANDLE = 7,
            IN_OPENRESULT_PREPAREBUFFER_INVALIDADDRESS = 8,
            IN_OPENRESULT_PREPAREBUFFER_UNKNOWNERROR = 1000,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            IN_OPENRESULT_ADDBUFFER_NOMEMORY = 9,
            IN_OPENRESULT_ADDBUFFER_STILLPLAYING = 10,
            IN_OPENRESULT_ADDBUFFER_UNPREPARED = 11,
            IN_OPENRESULT_ADDBUFFER_INVALIDHANDLE = 12,
            IN_OPENRESULT_ADDBUFFER_INVALIDSTRUCTURE = 13,
            IN_OPENRESULT_ADDBUFFER_UNKNOWNERROR = 2000,
            IN_OPENRESULT_INVALIDCLIENT = 101,
            IN_OPENRESULT_INVALIDPORT = 102,
            IN_OPENRESULT_WRONGTHREAD = 103,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            IN_OPENRESULT_NOTPERMITTED = 104,
            IN_OPENRESULT_UNKNOWNERROR = 10000
        }

        public enum IN_CLOSERESULT
        {
            IN_CLOSERESULT_OK = 0,
            IN_CLOSERESULT_RESET_INVALIDHANDLE = 1,
            IN_CLOSERESULT_CLOSE_STILLPLAYING = 2,
            IN_CLOSERESULT_CLOSE_INVALIDHANDLE = 3,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            IN_CLOSERESULT_CLOSE_NOMEMORY = 4,
            IN_CLOSERESULT_CLOSE_UNKNOWNERROR = 2000,
            IN_CLOSERESULT_UNPREPAREBUFFER_STILLPLAYING = 5,
            IN_CLOSERESULT_UNPREPAREBUFFER_INVALIDSTRUCTURE = 6,
            IN_CLOSERESULT_UNPREPAREBUFFER_INVALIDHANDLE = 7,
            IN_CLOSERESULT_UNPREPAREBUFFER_UNKNOWNERROR = 1000
        }

        public enum IN_RENEWSYSEXBUFFERRESULT
        {
            IN_RENEWSYSEXBUFFERRESULT_OK = 0,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_NOMEMORY = 1,
            IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDHANDLE = 2,
            IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDADDRESS = 3,
            IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_UNKNOWNERROR = 1000,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NoMemory)]
            IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_NOMEMORY = 4,
            IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_STILLPLAYING = 5,
            IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_UNPREPARED = 6,
            IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_INVALIDHANDLE = 7,
            IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_INVALIDSTRUCTURE = 8,
            IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_UNKNOWNERROR = 2000,
            IN_RENEWSYSEXBUFFERRESULT_UNPREPAREBUFFER_STILLPLAYING = 9,
            IN_RENEWSYSEXBUFFERRESULT_UNPREPAREBUFFER_INVALIDSTRUCTURE = 10,
            IN_RENEWSYSEXBUFFERRESULT_UNPREPAREBUFFER_INVALIDHANDLE = 11,
            IN_RENEWSYSEXBUFFERRESULT_UNPREPAREBUFFER_UNKNOWNERROR = 3000,
        }

        public enum IN_CONNECTRESULT
        {
            IN_CONNECTRESULT_OK = 0,
            IN_CONNECTRESULT_INVALIDHANDLE = 1,
            IN_CONNECTRESULT_UNKNOWNERROR = 101,
            IN_CONNECTRESULT_INVALIDPORT = 102,
            IN_CONNECTRESULT_WRONGTHREAD = 103,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            IN_CONNECTRESULT_NOTPERMITTED = 104,
            IN_CONNECTRESULT_UNKNOWNENDPOINT = 105,
            IN_CONNECTRESULT_WRONGENDPOINT = 106
        }

        public enum IN_DISCONNECTRESULT
        {
            IN_DISCONNECTRESULT_OK = 0,
            IN_DISCONNECTRESULT_INVALIDHANDLE = 1,
            IN_DISCONNECTRESULT_UNKNOWNERROR = 101,
            IN_DISCONNECTRESULT_INVALIDPORT = 102,
            IN_DISCONNECTRESULT_WRONGTHREAD = 103,
            [NativeApi.NativeErrorType(NativeApi.NativeErrorType.NotPermitted)]
            IN_DISCONNECTRESULT_NOTPERMITTED = 104,
            IN_DISCONNECTRESULT_UNKNOWNENDPOINT = 105,
            IN_DISCONNECTRESULT_WRONGENDPOINT = 106,
            IN_DISCONNECTRESULT_NOCONNECTION = 107
        }

        public enum IN_GETEVENTDATARESULT
        {
            IN_GETEVENTDATARESULT_OK = 0
        }

        public enum IN_GETSYSEXDATARESULT
        {
            IN_GETSYSEXDATARESULT_OK = 0
        }

        public enum IN_GETPROPERTYRESULT
        {
            IN_GETPROPERTYRESULT_OK = 0,
            IN_GETPROPERTYRESULT_UNKNOWNENDPOINT = 101,
            IN_GETPROPERTYRESULT_TOOLONG = 102,
            IN_GETPROPERTYRESULT_UNKNOWNPROPERTY = 103,
            IN_GETPROPERTYRESULT_UNKNOWNERROR = 104
        }

        #endregion

        #region Delegates

        public delegate void Callback_Win(IntPtr hMidi, NativeApi.MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);
        public delegate void Callback_Mac(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon);

        #endregion

        #region Extern functions

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int GetInputDevicesCount();

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_GETINFORESULT GetInputDeviceInfo(int deviceIndex, out IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int GetInputDeviceHashCode(IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool AreInputDevicesEqual(IntPtr info1, IntPtr info2);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceName(IntPtr info, out IntPtr value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceManufacturer(IntPtr info, out IntPtr value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceProduct(IntPtr info, out IntPtr value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceDriverVersion(IntPtr info, out int value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_OPENRESULT OpenInputDevice_Win(IntPtr info, IntPtr sessionHandle, Callback_Win callback, int sysExBufferSize, out IntPtr handle);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_OPENRESULT OpenInputDevice_Mac(IntPtr info, IntPtr sessionHandle, Callback_Mac callback, out IntPtr handle);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_CLOSERESULT CloseInputDevice(IntPtr handle);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_RENEWSYSEXBUFFERRESULT RenewInputDeviceSysExBuffer(IntPtr handle, int size);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_CONNECTRESULT ConnectToInputDevice(IntPtr handle);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_DISCONNECTRESULT DisconnectFromInputDevice(IntPtr handle);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_GETEVENTDATARESULT GetEventDataFromInputDevice(IntPtr packetList, int packetIndex, out IntPtr data, out int length, out int packetsCount);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_GETSYSEXDATARESULT GetInputDeviceSysExBufferData(IntPtr header, out IntPtr data, out int size);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool IsInputDevicePropertySupported(InputDeviceProperty property);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceUniqueId(IntPtr info, out int value);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IN_GETPROPERTYRESULT GetInputDeviceDriverOwner(IntPtr info, out IntPtr value);

        #endregion

        #region Methods

        public static int Api_GetDevicesCount()
        {
            return GetInputDevicesCount();
        }

        public static IN_GETINFORESULT Api_GetDeviceInfo(int deviceIndex, out IntPtr info)
        {
            return GetInputDeviceInfo(deviceIndex, out info);
        }

        public static int Api_GetDeviceHashCode(IntPtr info)
        {
            return GetInputDeviceHashCode(info);
        }

        public static bool Api_AreDevicesEqual(IntPtr info1, IntPtr info2)
        {
            return AreInputDevicesEqual(info1, info2);
        }

        public static IN_OPENRESULT Api_OpenDevice_Win(IntPtr info, IntPtr sessionHandle, Callback_Win callback, int sysExBufferSize, out IntPtr handle)
        {
            return OpenInputDevice_Win(info, sessionHandle, callback, sysExBufferSize, out handle);
        }

        public static IN_OPENRESULT Api_OpenDevice_Mac(IntPtr info, IntPtr sessionHandle, Callback_Mac callback, out IntPtr handle)
        {
            return OpenInputDevice_Mac(info, sessionHandle, callback, out handle);
        }

        public static IN_CLOSERESULT Api_CloseDevice(IntPtr handle)
        {
            return CloseInputDevice(handle);
        }

        public static IN_RENEWSYSEXBUFFERRESULT Api_RenewSysExBuffer(IntPtr handle, int size)
        {
            return RenewInputDeviceSysExBuffer(handle, size);
        }

        public static IN_CONNECTRESULT Api_Connect(IntPtr handle)
        {
            return ConnectToInputDevice(handle);
        }

        public static IN_DISCONNECTRESULT Api_Disconnect(IntPtr handle)
        {
            return DisconnectFromInputDevice(handle);
        }

        public static IN_GETEVENTDATARESULT Api_GetEventData(IntPtr packetList, int packetIndex, out IntPtr data, out int length, out int packetsCount)
        {
            return GetEventDataFromInputDevice(packetList, packetIndex, out data, out length, out packetsCount);
        }

        public static IN_GETSYSEXDATARESULT Api_GetSysExBufferData(IntPtr header, out IntPtr data, out int size)
        {
            return GetInputDeviceSysExBufferData(header, out data, out size);
        }

        public static bool Api_IsPropertySupported(InputDeviceProperty property)
        {
            return IsInputDevicePropertySupported(property);
        }

        public static IN_GETPROPERTYRESULT Api_GetDeviceName(IntPtr info, out string name)
        {
            IntPtr namePointer;
            var result = GetInputDeviceName(info, out namePointer);
            name = NativeApi.GetStringFromPointer(namePointer);
            return result;
        }

        public static IN_GETPROPERTYRESULT Api_GetDeviceManufacturer(IntPtr info, out string manufacturer)
        {
            IntPtr manufacturerPointer;
            var result = GetInputDeviceManufacturer(info, out manufacturerPointer);
            manufacturer = NativeApi.GetStringFromPointer(manufacturerPointer);
            return result;
        }

        public static IN_GETPROPERTYRESULT Api_GetDeviceProduct(IntPtr info, out string product)
        {
            IntPtr productPointer;
            var result = GetInputDeviceProduct(info, out productPointer);
            product = NativeApi.GetStringFromPointer(productPointer);
            return result;
        }

        public static IN_GETPROPERTYRESULT Api_GetDeviceDriverVersion(IntPtr info, out int driverVersion)
        {
            return GetInputDeviceDriverVersion(info, out driverVersion);
        }

        public static IN_GETPROPERTYRESULT Api_GetDeviceUniqueId(IntPtr info, out int uniqueId)
        {
            return GetInputDeviceUniqueId(info, out uniqueId);
        }

        public static IN_GETPROPERTYRESULT Api_GetDeviceDriverOwner(IntPtr info, out string driverOwner)
        {
            IntPtr driverOwnerPointer;
            var result = GetInputDeviceDriverOwner(info, out driverOwnerPointer);
            driverOwner = NativeApi.GetStringFromPointer(driverOwnerPointer);
            return result;
        }

        #endregion
    }
}
