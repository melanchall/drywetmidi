using System;

namespace Melanchall.DryWetMidi.Devices
{
    internal abstract class MidiDevicesSessionApi
    {
        #region Nested enums

        public enum SESSION_OPENRESULT
        {
            SESSION_OPENRESULT_OK = 0
        }

        public enum SESSION_CLOSERESULT
        {
            SESSION_CLOSERESULT_OK = 0
        }

        #endregion

        #region Methods

        public abstract SESSION_OPENRESULT Api_OpenSession(IntPtr name, out IntPtr handle);

        public abstract SESSION_CLOSERESULT Api_CloseSession(IntPtr handle);

        #endregion
    }
}
