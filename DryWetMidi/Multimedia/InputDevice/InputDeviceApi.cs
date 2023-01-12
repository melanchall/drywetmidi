using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal abstract class InputDeviceApi : NativeApi
    {
        #region Nested enums

        public enum IN_GETINFORESULT
        {
            IN_GETINFORESULT_OK = 0,

            IN_GETINFORESULT_BADDEVICEID = 1,
            IN_GETINFORESULT_INVALIDSTRUCTURE = 2,
            IN_GETINFORESULT_NODRIVER = 3,
            [NativeErrorType(NativeErrorType.NoMemory)]
            IN_GETINFORESULT_NOMEMORY = 4,

            IN_GETINFORESULT_UNKNOWNERROR = 1000
        }

        public enum IN_OPENRESULT
        {
            IN_OPENRESULT_OK = 0,
            [NativeErrorType(NativeErrorType.InUse)]
            IN_OPENRESULT_ALLOCATED = 1,
            IN_OPENRESULT_BADDEVICEID = 2,
            IN_OPENRESULT_INVALIDFLAG = 3,
            IN_OPENRESULT_INVALIDSTRUCTURE = 4,
            [NativeErrorType(NativeErrorType.NoMemory)]
            IN_OPENRESULT_NOMEMORY = 5,
            [NativeErrorType(NativeErrorType.NoMemory)]
            IN_OPENRESULT_PREPAREBUFFER_NOMEMORY = 6,
            IN_OPENRESULT_PREPAREBUFFER_INVALIDHANDLE = 7,
            IN_OPENRESULT_PREPAREBUFFER_INVALIDADDRESS = 8,
            IN_OPENRESULT_PREPAREBUFFER_UNKNOWNERROR = 1000,
            [NativeErrorType(NativeErrorType.NoMemory)]
            IN_OPENRESULT_ADDBUFFER_NOMEMORY = 9,
            IN_OPENRESULT_ADDBUFFER_STILLPLAYING = 10,
            IN_OPENRESULT_ADDBUFFER_UNPREPARED = 11,
            IN_OPENRESULT_ADDBUFFER_INVALIDHANDLE = 12,
            IN_OPENRESULT_ADDBUFFER_INVALIDSTRUCTURE = 13,
            IN_OPENRESULT_ADDBUFFER_UNKNOWNERROR = 2000,
            IN_OPENRESULT_INVALIDCLIENT = 101,
            IN_OPENRESULT_INVALIDPORT = 102,
            IN_OPENRESULT_WRONGTHREAD = 103,
            [NativeErrorType(NativeErrorType.NotPermitted)]
            IN_OPENRESULT_NOTPERMITTED = 104,
            IN_OPENRESULT_UNKNOWNERROR = 10000
        }

        public enum IN_CLOSERESULT
        {
            IN_CLOSERESULT_OK = 0,
            IN_CLOSERESULT_RESET_INVALIDHANDLE = 1,
            IN_CLOSERESULT_CLOSE_STILLPLAYING = 2,
            IN_CLOSERESULT_CLOSE_INVALIDHANDLE = 3,
            [NativeErrorType(NativeErrorType.NoMemory)]
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
            [NativeErrorType(NativeErrorType.NoMemory)]
            IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_NOMEMORY = 1,
            IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDHANDLE = 2,
            IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDADDRESS = 3,
            IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_UNKNOWNERROR = 1000,
            [NativeErrorType(NativeErrorType.NoMemory)]
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
            [NativeErrorType(NativeErrorType.NotPermitted)]
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
            [NativeErrorType(NativeErrorType.NotPermitted)]
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

        public delegate void Callback_Win(IntPtr hMidi, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);
        public delegate void Callback_Mac(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon);

        #endregion

        #region Methods

        public abstract int Api_GetDevicesCount();

        public abstract IN_GETINFORESULT Api_GetDeviceInfo(int deviceIndex, out IntPtr info);

        public abstract int Api_GetDeviceHashCode(IntPtr info);

        public abstract bool Api_AreDevicesEqual(IntPtr info1, IntPtr info2);

        public abstract IN_OPENRESULT Api_OpenDevice_Win(IntPtr info, IntPtr sessionHandle, Callback_Win callback, int sysExBufferSize, out IntPtr handle);

        public abstract IN_OPENRESULT Api_OpenDevice_Mac(IntPtr info, IntPtr sessionHandle, Callback_Mac callback, out IntPtr handle);

        public abstract IN_CLOSERESULT Api_CloseDevice(IntPtr handle);

        public abstract IN_RENEWSYSEXBUFFERRESULT Api_RenewSysExBuffer(IntPtr handle, int size);

        public abstract IN_CONNECTRESULT Api_Connect(IntPtr handle);

        public abstract IN_DISCONNECTRESULT Api_Disconnect(IntPtr handle);

        public abstract IN_GETEVENTDATARESULT Api_GetEventData(IntPtr packetList, int packetIndex, out IntPtr data, out int length, out int packetsCount);

        public abstract IN_GETSYSEXDATARESULT Api_GetSysExBufferData(IntPtr header, out IntPtr data, out int size);

        public abstract bool Api_IsPropertySupported(InputDeviceProperty property);

        public abstract IN_GETPROPERTYRESULT Api_GetDeviceName(IntPtr info, out string name);

        public abstract IN_GETPROPERTYRESULT Api_GetDeviceManufacturer(IntPtr info, out string manufacturer);

        public abstract IN_GETPROPERTYRESULT Api_GetDeviceProduct(IntPtr info, out string product);

        public abstract IN_GETPROPERTYRESULT Api_GetDeviceDriverVersion(IntPtr info, out int driverVersion);

        public abstract IN_GETPROPERTYRESULT Api_GetDeviceUniqueId(IntPtr info, out int uniqueId);

        public abstract IN_GETPROPERTYRESULT Api_GetDeviceDriverOwner(IntPtr info, out string driverOwner);

        #endregion
    }
}
