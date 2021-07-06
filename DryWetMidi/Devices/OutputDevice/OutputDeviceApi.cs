using System;

namespace Melanchall.DryWetMidi.Devices
{
    internal abstract class OutputDeviceApi
    {
        #region Nested enums

        public enum API_TYPE
        {
            API_TYPE_WINMM = 0,
            API_TYPE_APPLE = 1
        }

        public enum OUT_GETINFORESULT
        {
            OUT_GETINFORESULT_OK = 0,
            OUT_GETINFORESULT_BADDEVICEID = 1,
            OUT_GETINFORESULT_INVALIDSTRUCTURE = 2,
            OUT_GETINFORESULT_NODRIVER = 3,
            OUT_GETINFORESULT_NOMEMORY = 4
        }

        public enum OUT_OPENRESULT
        {
            OUT_OPENRESULT_OK = 0,
            OUT_OPENRESULT_ALLOCATED = 1,
            OUT_OPENRESULT_BADDEVICEID = 2,
            OUT_OPENRESULT_INVALIDFLAG = 3,
            OUT_OPENRESULT_INVALIDSTRUCTURE = 4,
            OUT_OPENRESULT_NOMEMORY = 5
        }

        #endregion

        #region Delegates

        public delegate void Callback_Winmm(IntPtr hMidi, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

        #endregion

        #region Methods

        public abstract API_TYPE Api_GetApiType();

        public abstract int Api_GetDevicesCount();

        public abstract OUT_GETINFORESULT Api_GetDeviceInfo(int deviceIndex, out IntPtr info);

        public abstract string Api_GetDeviceName(IntPtr info);

        public abstract string Api_GetDeviceManufacturer(IntPtr info);

        public abstract string Api_GetDeviceProduct(IntPtr info);

        public abstract uint Api_GetDeviceDriverVersion(IntPtr info);

        public abstract OUT_OPENRESULT Api_OpenDevice_Winmm(IntPtr info, IntPtr sessionHandle, Callback_Winmm callback, out IntPtr handle);

        public abstract OUT_OPENRESULT Api_OpenDevice_Apple(IntPtr info, IntPtr sessionHandle, out IntPtr handle);

        // TODO: remove
        public abstract IntPtr Api_GetHandle(IntPtr handle);

        public abstract void Api_CloseDevice(IntPtr handle);

        public abstract int Api_SendShortEvent(IntPtr handle, int message);

        #endregion
    }
}
