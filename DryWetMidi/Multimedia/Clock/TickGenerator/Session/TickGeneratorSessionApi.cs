using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class TickGeneratorSessionApi
    {
        #region Nested enums

        public enum TGSESSION_OPENRESULT
        {
            TGSESSION_OPENRESULT_OK = 0,

            TGSESSION_OPENRESULT_FAILEDTOGETTIMEBASEINFO = 101,
            TGSESSION_OPENRESULT_FAILEDTOSETREALTIMEPRIORITY = 102,
            TGSESSION_OPENRESULT_THREADSTARTERROR = 103
        }

        public enum TGSESSION_CLOSERESULT
        {
            TGSESSION_CLOSERESULT_OK = 0
        }

        #endregion

        #region Extern functions

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern TGSESSION_OPENRESULT OpenTickGeneratorSession(out IntPtr handle);

        #endregion

        #region Methods

        public static TGSESSION_OPENRESULT Api_OpenSession(out IntPtr handle)
        {
            return OpenTickGeneratorSession(out handle);
        }

        #endregion
    }
}
