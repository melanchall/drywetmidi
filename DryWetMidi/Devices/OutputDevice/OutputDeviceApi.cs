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

        public enum OUT_CLOSERESULT
        {
            OUT_CLOSERESULT_OK = 0,
            OUT_CLOSERESULT_RESET_INVALIDHANDLE = 1,
            OUT_CLOSERESULT_CLOSE_STILLPLAYING = 2,
            OUT_CLOSERESULT_CLOSE_INVALIDHANDLE = 3,
            OUT_CLOSERESULT_CLOSE_NOMEMORY = 4
        }

        public enum OUT_SENDSHORTRESULT
        {
            OUT_SENDSHORTRESULT_OK = 0,
            OUT_SENDSHORTRESULT_BADOPENMODE = 1,
            OUT_SENDSHORTRESULT_NOTREADY = 2,
            OUT_SENDSHORTRESULT_INVALIDHANDLE = 3,
            OUT_SENDSHORTRESULT_INVALIDCLIENT = 101,
            OUT_SENDSHORTRESULT_INVALIDPORT = 102,
            OUT_SENDSHORTRESULT_WRONGENDPOINT = 103,
            OUT_SENDSHORTRESULT_UNKNOWNENDPOINT = 104,
            OUT_SENDSHORTRESULT_COMMUNICATIONERROR = 105,
            OUT_SENDSHORTRESULT_SERVERSTARTERROR = 106,
            OUT_SENDSHORTRESULT_WRONGTHREAD = 107,
            OUT_SENDSHORTRESULT_NOTPERMITTED = 108,
            OUT_SENDSHORTRESULT_UNKNOWNERROR = 109
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

        public abstract OUT_CLOSERESULT Api_CloseDevice(IntPtr handle);

        public abstract OUT_SENDSHORTRESULT Api_SendShortEvent(IntPtr handle, int message);

        public static void HandleResult(OUT_CLOSERESULT result)
        {
            if (result != OUT_CLOSERESULT.OUT_CLOSERESULT_OK)
                throw new MidiDeviceException(GetErrorDescription(result), (int)result);
        }

        public static void HandleResult(OUT_SENDSHORTRESULT result)
        {
            if (result != OUT_SENDSHORTRESULT.OUT_SENDSHORTRESULT_OK)
                throw new MidiDeviceException(GetErrorDescription(result), (int)result);
        }

        private static string GetErrorDescription(OUT_CLOSERESULT result)
        {
            switch (result)
            {
                case OUT_CLOSERESULT.OUT_CLOSERESULT_CLOSE_NOMEMORY:
                    return $"There is no memory in the system to close the device ({result}).";
            }

            return GetInternalErrorDescription(result);
        }

        private static string GetErrorDescription(OUT_SENDSHORTRESULT result)
        {
            switch (result)
            {
                case OUT_SENDSHORTRESULT.OUT_SENDSHORTRESULT_NOTREADY:
                    return $"The hardware is busy with other data ({result}).";
                case OUT_SENDSHORTRESULT.OUT_SENDSHORTRESULT_COMMUNICATIONERROR:
                case OUT_SENDSHORTRESULT.OUT_SENDSHORTRESULT_SERVERSTARTERROR:
                    return $"MIDI server error ({result}).";
                case OUT_SENDSHORTRESULT.OUT_SENDSHORTRESULT_NOTPERMITTED:
                    return $"The process doesn’t have privileges for the requested operation ({result}).";
            }

            return GetInternalErrorDescription(result);
        }

        private static string GetInternalErrorDescription(object result)
        {
            return $"Internal error ({result}).";
        }

        #endregion
    }
}
