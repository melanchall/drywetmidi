using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal abstract class TickGeneratorSessionApi : NativeApi
    {
        #region Nested enums

        public enum TGSESSION_OPENRESULT
        {
            TGSESSION_OPENRESULT_OK = 0,

            TGSESSION_OPENRESULT_FAILEDTOGETTIMEBASEINFO = 101,
            TGSESSION_OPENRESULT_FAILEDTOSETREALTIMEPRIORITY = 102
        }

        public enum TGSESSION_CLOSERESULT
        {
            TGSESSION_CLOSERESULT_OK = 0
        }

        #endregion

        #region Methods

        public abstract TGSESSION_OPENRESULT Api_OpenSession(out IntPtr handle);

        #endregion
    }
}
