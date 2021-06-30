using System;

namespace Melanchall.DryWetMidi.Devices
{
    internal abstract class InputDeviceApi
    {
        #region Nested enums

        public enum API_TYPE
        {
            API_TYPE_WINMM = 0,
            API_TYPE_APPLE = 1
        }

        public enum IN_GETINFORESULT
        {
            IN_GETINFORESULT_OK = 0,
            IN_GETINFORESULT_BADDEVICEID = 1,
            IN_GETINFORESULT_INVALIDSTRUCTURE = 2,
            IN_GETINFORESULT_NODRIVER = 3,
            IN_GETINFORESULT_NOMEMORY = 4
        }

        public enum IN_OPENRESULT
        {
            IN_OPENRESULT_OK = 0,
            IN_OPENRESULT_ALLOCATED = 1,
            IN_OPENRESULT_BADDEVICEID = 2,
            IN_OPENRESULT_INVALIDFLAG = 3,
            IN_OPENRESULT_INVALIDSTRUCTURE = 4,
            IN_OPENRESULT_NOMEMORY = 5,
            IN_OPENRESULT_PREPAREBUFFER_NOMEMORY = 6,
            IN_OPENRESULT_PREPAREBUFFER_INVALIDHANDLE = 7,
            IN_OPENRESULT_PREPAREBUFFER_INVALIDADDRESS = 8,
            IN_OPENRESULT_ADDBUFFER_NOMEMORY = 9,
            IN_OPENRESULT_ADDBUFFER_STILLPLAYING = 10,
            IN_OPENRESULT_ADDBUFFER_UNPREPARED = 11,
            IN_OPENRESULT_ADDBUFFER_INVALIDHANDLE = 12,
            IN_OPENRESULT_ADDBUFFER_INVALIDSTRUCTURE = 13
        }

        public enum IN_CLOSERESULT
        {
            IN_CLOSERESULT_OK = 0,
            IN_CLOSERESULT_RESET_INVALIDHANDLE = 1,
            IN_CLOSERESULT_CLOSE_STILLPLAYING = 2,
            IN_CLOSERESULT_CLOSE_INVALIDHANDLE = 3,
            IN_CLOSERESULT_CLOSE_NOMEMORY = 4,
            IN_CLOSERESULT_UNPREPAREBUFFER_STILLPLAYING = 5,
            IN_CLOSERESULT_UNPREPAREBUFFER_INVALIDSTRUCTURE = 6,
            IN_CLOSERESULT_UNPREPAREBUFFER_INVALIDHANDLE = 7
        }

        public enum IN_RENEWSYSEXBUFFERRESULT
        {
            IN_RENEWSYSEXBUFFERRESULT_OK = 0,
            IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_NOMEMORY = 1,
            IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDHANDLE = 2,
            IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDADDRESS = 3,
            IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_NOMEMORY = 4,
            IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_STILLPLAYING = 5,
            IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_UNPREPARED = 6,
            IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_INVALIDHANDLE = 7,
            IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_INVALIDSTRUCTURE = 8,
            IN_RENEWSYSEXBUFFERRESULT_UNPREPAREBUFFER_STILLPLAYING = 9,
            IN_RENEWSYSEXBUFFERRESULT_UNPREPAREBUFFER_INVALIDSTRUCTURE = 10,
            IN_RENEWSYSEXBUFFERRESULT_UNPREPAREBUFFER_INVALIDHANDLE = 11
        }

        public enum IN_CONNECTRESULT
        {
            IN_CONNECTRESULT_OK = 0,
            IN_CONNECTRESULT_INVALIDHANDLE = 1
        }

        public enum IN_DISCONNECTRESULT
        {
            IN_DISCONNECTRESULT_OK = 0,
            IN_DISCONNECTRESULT_INVALIDHANDLE = 1
        }

        #endregion

        #region Delegates

        public delegate void Callback_Winmm(IntPtr hMidi, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

        #endregion

        #region Methods

        public abstract API_TYPE Api_GetApiType();

        public abstract int Api_GetDevicesCount();

        public abstract IN_GETINFORESULT Api_GetDeviceInfo(int deviceIndex, out IntPtr info);

        public abstract string Api_GetDeviceName(IntPtr info);

        public abstract string Api_GetDeviceManufacturer(IntPtr info);

        public abstract string Api_GetDeviceProduct(IntPtr info);

        public abstract uint Api_GetDeviceDriverVersion(IntPtr info);

        public abstract IN_OPENRESULT Api_OpenDevice_Winmm(IntPtr info, Callback_Winmm callback, int sysExBufferSize, out IntPtr handle);

        public abstract IN_CLOSERESULT Api_CloseDevice(IntPtr handle);

        public abstract IN_RENEWSYSEXBUFFERRESULT Api_RenewSysExBuffer(IntPtr handle, int size);

        public abstract IN_CONNECTRESULT Api_Connect(IntPtr handle);

        public abstract IN_DISCONNECTRESULT Api_Disconnect(IntPtr handle);

        #endregion
    }
}
