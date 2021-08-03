using System;

namespace Melanchall.DryWetMidi.Devices
{
    internal abstract class OutputDeviceApi : NativeApi
    {
        #region Nested enums

        public enum OUT_GETINFORESULT
        {
            OUT_GETINFORESULT_OK = 0,
            OUT_GETINFORESULT_BADDEVICEID = 1,
            OUT_GETINFORESULT_INVALIDSTRUCTURE = 2,
            OUT_GETINFORESULT_NODRIVER = 3,
            [NativeErrorType(NativeErrorType.NoMemory)]
            OUT_GETINFORESULT_NOMEMORY = 4,
            OUT_GETINFORESULT_NAME_UNKNOWNENDPOINT = 101,
            OUT_GETINFORESULT_NAME_TOOLONG = 102,
            OUT_GETINFORESULT_MANUFACTURER_UNKNOWNENDPOINT = 103,
            OUT_GETINFORESULT_MANUFACTURER_TOOLONG = 104,
            OUT_GETINFORESULT_PRODUCT_UNKNOWNENDPOINT = 105,
            OUT_GETINFORESULT_PRODUCT_TOOLONG = 106,
            OUT_GETINFORESULT_UNKNOWNERROR = 107,
            OUT_GETINFORESULT_DRIVERVERSION_UNKNOWNENDPOINT = 108
        }

        public enum OUT_OPENRESULT
        {
            OUT_OPENRESULT_OK = 0,
            [NativeErrorType(NativeErrorType.InUse)]
            OUT_OPENRESULT_ALLOCATED = 1,
            OUT_OPENRESULT_BADDEVICEID = 2,
            OUT_OPENRESULT_INVALIDFLAG = 3,
            OUT_OPENRESULT_INVALIDSTRUCTURE = 4,
            [NativeErrorType(NativeErrorType.NoMemory)]
            OUT_OPENRESULT_NOMEMORY = 5,
            OUT_OPENRESULT_INVALIDCLIENT = 101,
            OUT_OPENRESULT_INVALIDPORT = 102,
            OUT_OPENRESULT_WRONGTHREAD = 103,
            [NativeErrorType(NativeErrorType.NotPermitted)]
            OUT_OPENRESULT_NOTPERMITTED = 104,
            OUT_OPENRESULT_UNKNOWNERROR = 105
        }

        public enum OUT_CLOSERESULT
        {
            OUT_CLOSERESULT_OK = 0,
            OUT_CLOSERESULT_RESET_INVALIDHANDLE = 1,
            OUT_CLOSERESULT_CLOSE_STILLPLAYING = 2,
            OUT_CLOSERESULT_CLOSE_INVALIDHANDLE = 3,
            [NativeErrorType(NativeErrorType.NoMemory)]
            OUT_CLOSERESULT_CLOSE_NOMEMORY = 4
        }

        public enum OUT_SENDSHORTRESULT
        {
            OUT_SENDSHORTRESULT_OK = 0,
            OUT_SENDSHORTRESULT_BADOPENMODE = 1,
            [NativeErrorType(NativeErrorType.Busy)]
            OUT_SENDSHORTRESULT_NOTREADY = 2,
            OUT_SENDSHORTRESULT_INVALIDHANDLE = 3,
            OUT_SENDSHORTRESULT_INVALIDCLIENT = 101,
            OUT_SENDSHORTRESULT_INVALIDPORT = 102,
            OUT_SENDSHORTRESULT_WRONGENDPOINT = 103,
            OUT_SENDSHORTRESULT_UNKNOWNENDPOINT = 104,
            OUT_SENDSHORTRESULT_COMMUNICATIONERROR = 105,
            OUT_SENDSHORTRESULT_SERVERSTARTERROR = 106,
            OUT_SENDSHORTRESULT_WRONGTHREAD = 107,
            [NativeErrorType(NativeErrorType.NotPermitted)]
            OUT_SENDSHORTRESULT_NOTPERMITTED = 108,
            OUT_SENDSHORTRESULT_UNKNOWNERROR = 109
        }

        public enum OUT_SENDSYSEXRESULT
        {
            OUT_SENDSYSEXRESULT_OK = 0,
            OUT_SENDSYSEXRESULT_PREPAREBUFFER_INVALIDHANDLE = 1,
            OUT_SENDSYSEXRESULT_PREPAREBUFFER_INVALIDADDRESS = 2,
            [NativeErrorType(NativeErrorType.NoMemory)]
            OUT_SENDSYSEXRESULT_PREPAREBUFFER_NOMEMORY = 3,
            [NativeErrorType(NativeErrorType.Busy)]
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
            [NativeErrorType(NativeErrorType.NotPermitted)]
            OUT_SENDSYSEXRESULT_NOTPERMITTED = 108,
            OUT_SENDSYSEXRESULT_UNKNOWNERROR = 109
        }

        public enum OUT_GETSYSEXDATARESULT
        {
            OUT_GETSYSEXDATARESULT_OK = 0,
            OUT_GETSYSEXDATARESULT_STILLPLAYING = 1,
            OUT_GETSYSEXDATARESULT_INVALIDSTRUCTURE = 2,
            OUT_GETSYSEXDATARESULT_INVALIDHANDLE = 3
        }

        #endregion

        #region Delegates

        public delegate void Callback_Win(IntPtr hMidi, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

        #endregion

        #region Methods

        public abstract int Api_GetDevicesCount();

        public abstract OUT_GETINFORESULT Api_GetDeviceInfo(int deviceIndex, out IntPtr info);

        public abstract string Api_GetDeviceName(IntPtr info);

        public abstract string Api_GetDeviceManufacturer(IntPtr info);

        public abstract string Api_GetDeviceProduct(IntPtr info);

        public abstract int Api_GetDeviceDriverVersion(IntPtr info);

        public abstract OUT_OPENRESULT Api_OpenDevice_Win(IntPtr info, IntPtr sessionHandle, Callback_Win callback, out IntPtr handle);

        public abstract OUT_OPENRESULT Api_OpenDevice_Mac(IntPtr info, IntPtr sessionHandle, out IntPtr handle);

        public abstract OUT_CLOSERESULT Api_CloseDevice(IntPtr handle);

        public abstract OUT_SENDSHORTRESULT Api_SendShortEvent(IntPtr handle, int message);

        public abstract OUT_SENDSYSEXRESULT Api_SendSysExEvent_Mac(IntPtr handle, byte[] data, ushort dataSize);

        public abstract OUT_SENDSYSEXRESULT Api_SendSysExEvent_Win(IntPtr handle, IntPtr data, int size);

        public abstract OUT_GETSYSEXDATARESULT Api_GetSysExBufferData(IntPtr handle, IntPtr header, out IntPtr data, out int size);

        #endregion
    }
}
