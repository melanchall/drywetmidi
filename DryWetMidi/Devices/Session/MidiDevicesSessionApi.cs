using System;

namespace Melanchall.DryWetMidi.Devices
{
    internal abstract class MidiDevicesSessionApi
    {
        #region Nested enums

        public enum SESSION_OPENRESULT
        {
            SESSION_OPENRESULT_OK = 0,
            SESSION_OPENRESULT_SERVERSTARTERROR = 101,
            SESSION_OPENRESULT_WRONGTHREAD = 102,
            SESSION_OPENRESULT_NOTPERMITTED = 103,
            SESSION_OPENRESULT_UNKNOWNERROR = 104
        }

        public enum SESSION_CLOSERESULT
        {
            SESSION_CLOSERESULT_OK = 0
        }

        #endregion

        #region Methods

        public abstract SESSION_OPENRESULT Api_OpenSession(IntPtr name, out IntPtr handle);

        public abstract SESSION_CLOSERESULT Api_CloseSession(IntPtr handle);

        public static void HandleResult(SESSION_OPENRESULT result)
        {
            if (result != SESSION_OPENRESULT.SESSION_OPENRESULT_OK)
                throw new MidiDeviceException(GetErrorDescription(result), (int)result);
        }

        private static string GetErrorDescription(SESSION_OPENRESULT result)
        {
            switch (result)
            {
                case SESSION_OPENRESULT.SESSION_OPENRESULT_SERVERSTARTERROR:
                    return $"MIDI server error ({result}).";
                case SESSION_OPENRESULT.SESSION_OPENRESULT_NOTPERMITTED:
                    return $"The process doesn’t have privileges for the requested operation ({result}).";
            }

            return GetInternalErrorDescription(result);
        }

        private static string GetInternalErrorDescription(object result)
        {
            return $"Internal error on open MIDI device session ({result}).";
        }

        #endregion
    }
}
