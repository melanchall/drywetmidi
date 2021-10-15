using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal abstract class VirtualDeviceApi : NativeApi
    {
        #region Nested enums

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

        public delegate void Callback_Mac(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon);

        #endregion

        #region Methods

        public abstract VIRTUAL_OPENRESULT Api_OpenDevice_Mac(string name, IntPtr sessionHandle, Callback_Mac callback, out IntPtr info);

        public abstract VIRTUAL_CLOSERESULT Api_CloseDevice(IntPtr info);

        public abstract VIRTUAL_SENDBACKRESULT Api_SendDataBack(IntPtr pktlist, IntPtr readProcRefCon);

        public abstract IntPtr Api_GetInputDeviceInfo(IntPtr info);

        public abstract IntPtr Api_GetOutputDeviceInfo(IntPtr info);

        #endregion
    }
}
