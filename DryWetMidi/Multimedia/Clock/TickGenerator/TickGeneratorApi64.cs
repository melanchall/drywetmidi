using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class TickGeneratorApi64 : TickGeneratorApi
    {
        #region Constants

        private const string LibraryName = LibraryName64;

        #endregion

        #region Extern functions

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern TG_STARTRESULT StartHighPrecisionTickGenerator_Win(int interval, IntPtr sessionHandle, TimerCallback_Win callback, out IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern TG_STARTRESULT StartHighPrecisionTickGenerator_Mac(int interval, IntPtr sessionHandle, TimerCallback_Mac callback, out IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern TG_STOPRESULT StopHighPrecisionTickGenerator(IntPtr sessionHandle, IntPtr info);

        #endregion

        #region Methods

        public override TG_STARTRESULT Api_StartHighPrecisionTickGenerator_Win(int interval, IntPtr sessionHandle, TimerCallback_Win callback, out IntPtr info)
        {
            return StartHighPrecisionTickGenerator_Win(interval, sessionHandle, callback, out info);
        }

        public override TG_STARTRESULT Api_StartHighPrecisionTickGenerator_Mac(int interval, IntPtr sessionHandle, TimerCallback_Mac callback, out IntPtr info)
        {
            return StartHighPrecisionTickGenerator_Mac(interval, sessionHandle, callback, out info);
        }

        public override TG_STOPRESULT Api_StopHighPrecisionTickGenerator(IntPtr sessionHandle, IntPtr info)
        {
            return StopHighPrecisionTickGenerator(sessionHandle, info);
        }

        #endregion
    }
}
