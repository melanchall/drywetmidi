using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class TickGeneratorApi64 : TickGeneratorApi
    {
        #region Constants

        private const string LibraryName = LibraryName64;

        #endregion

        #region Extern functions

        [DllImport(LibraryName, ExactSpelling = true)]
        public static extern TG_STARTRESULT StartHighPrecisionTickGenerator_Win(int interval, TimerCallback_Win callback, out IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        public static extern TG_STARTRESULT StartHighPrecisionTickGenerator_Mac(int interval, TimerCallback_Mac callback, out IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        public static extern TG_STOPRESULT StopHighPrecisionTickGenerator(IntPtr info);

        #endregion

        #region Methods

        public override TG_STARTRESULT Api_StartHighPrecisionTickGenerator_Win(int interval, TimerCallback_Win callback, out IntPtr info)
        {
            return StartHighPrecisionTickGenerator_Win(interval, callback, out info);
        }

        public override TG_STARTRESULT Api_StartHighPrecisionTickGenerator_Mac(int interval, TimerCallback_Mac callback, out IntPtr info)
        {
            return StartHighPrecisionTickGenerator_Mac(interval, callback, out info);
        }

        public override TG_STOPRESULT Api_StopHighPrecisionTickGenerator(IntPtr info)
        {
            return StopHighPrecisionTickGenerator(info);
        }

        #endregion
    }
}
