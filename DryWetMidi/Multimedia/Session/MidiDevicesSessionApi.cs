using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal abstract class MidiDevicesSessionApi : NativeApi
    {
        #region Nested enums

        public enum SESSION_OPENRESULT
        {
            SESSION_OPENRESULT_OK = 0,
            SESSION_OPENRESULT_SERVERSTARTERROR = 101,
            SESSION_OPENRESULT_WRONGTHREAD = 102,
            [NativeErrorType(NativeErrorType.NotPermitted)]
            SESSION_OPENRESULT_NOTPERMITTED = 103,
            SESSION_OPENRESULT_UNKNOWNERROR = 104
        }

        public enum SESSION_CLOSERESULT
        {
            SESSION_CLOSERESULT_OK = 0
        }

        #endregion

        #region Delegates

        public delegate void InputDeviceCallback(IntPtr info, bool operation);
        public delegate void OutputDeviceCallback(IntPtr info, bool operation);

        #endregion

        #region Methods

        public abstract SESSION_OPENRESULT Api_OpenSession_Mac(
            IntPtr name,
            InputDeviceCallback inputDeviceCallback,
            OutputDeviceCallback outputDeviceCallback,
            out IntPtr handle);

        public abstract SESSION_OPENRESULT Api_OpenSession_Win(IntPtr name, out IntPtr handle);

        public abstract SESSION_CLOSERESULT Api_CloseSession(IntPtr handle);

        #endregion
    }
}
