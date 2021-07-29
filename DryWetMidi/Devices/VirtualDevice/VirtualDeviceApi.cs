using System;

namespace Melanchall.DryWetMidi.Devices
{
    internal abstract class VirtualDeviceApi : NativeApi
    {
        #region Nested enums

        public enum API_TYPE
        {
            API_TYPE_WINMM = 0,
            API_TYPE_APPLE = 1
        }

        public enum VIRTUAL_OPENRESULT
        {
            VIRTUAL_OPENRESULT_OK = 0,
            [NativeErrorType(NativeErrorType.NotPermitted)]
            VIRTUAL_OPENRESULT_CREATESOURCE_NOTPERMITTED = 101,
            VIRTUAL_OPENRESULT_CREATESOURCE_SERVERSTARTERROR = 102,
            VIRTUAL_OPENRESULT_CREATESOURCE_WRONGTHREAD = 103,
            VIRTUAL_OPENRESULT_CREATESOURCE_UNKNOWNERROR = 104,
            [NativeErrorType(NativeErrorType.NotPermitted)]
            VIRTUAL_OPENRESULT_CREATEDESTINATION_NOTPERMITTED = 105,
            VIRTUAL_OPENRESULT_CREATEDESTINATION_SERVERSTARTERROR = 106,
            VIRTUAL_OPENRESULT_CREATEDESTINATION_WRONGTHREAD = 107,
            VIRTUAL_OPENRESULT_CREATEDESTINATION_UNKNOWNERROR = 108
        }

        public enum VIRTUAL_CLOSERESULT
        {
            VIRTUAL_CLOSERESULT_OK = 0,
            VIRTUAL_CLOSERESULT_DISPOSESOURCE_UNKNOWNENDPOINT = 101,
            [NativeErrorType(NativeErrorType.NotPermitted)]
            VIRTUAL_CLOSERESULT_DISPOSESOURCE_NOTPERMITTED = 102,
            VIRTUAL_CLOSERESULT_DISPOSESOURCE_UNKNOWNERROR = 103,
            VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_UNKNOWNENDPOINT = 104,
            [NativeErrorType(NativeErrorType.NotPermitted)]
            VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_NOTPERMITTED = 105,
            VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_UNKNOWNERROR = 106
        }

        public enum VIRTUAL_SENDBACKRESULT
        {
            VIRTUAL_SENDBACKRESULT_OK = 0,
            VIRTUAL_SENDBACKRESULT_UNKNOWNERROR_TE = 1,
            VIRTUAL_SENDBACKRESULT_UNKNOWNENDPOINT = 101,
            VIRTUAL_SENDBACKRESULT_WRONGENDPOINT = 102,
            [NativeErrorType(NativeErrorType.NotPermitted)]
            VIRTUAL_SENDBACKRESULT_NOTPERMITTED = 103,
            VIRTUAL_SENDBACKRESULT_SERVERSTARTERROR = 104,
            VIRTUAL_SENDBACKRESULT_WRONGTHREAD = 105,
            VIRTUAL_SENDBACKRESULT_UNKNOWNERROR = 106,
            VIRTUAL_SENDBACKRESULT_MESSAGESENDERROR = 107
        }

        #endregion

        #region Delegates

        public delegate void Callback_Apple(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon);
        public delegate void Callback_Te(IntPtr midiPort, IntPtr midiDataBytes, uint length, IntPtr dwCallbackInstance);

        #endregion

        #region Methods

        public abstract API_TYPE Api_GetApiType();

        public abstract VIRTUAL_OPENRESULT Api_OpenDevice_Apple(string name, IntPtr sessionHandle, Callback_Apple callback, out IntPtr info);

        public abstract VIRTUAL_OPENRESULT Api_OpenDevice_Te(string name, IntPtr sessionHandle, Callback_Te callback, out IntPtr info);

        public abstract VIRTUAL_CLOSERESULT Api_CloseDevice(IntPtr info);

        public abstract VIRTUAL_SENDBACKRESULT Api_SendDataBack(IntPtr pktlist, IntPtr readProcRefCon);

        public abstract VIRTUAL_SENDBACKRESULT Api_SendDataBack_Te(IntPtr midiPort, IntPtr midiDataBytes, uint length);

        public abstract IntPtr Api_GetInputDeviceInfo(IntPtr info);

        public abstract IntPtr Api_GetOutputDeviceInfo(IntPtr info);

        #endregion
    }
}
