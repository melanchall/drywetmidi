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
        private static extern API_TYPE GetApiType();

        [DllImport(LibraryName, ExactSpelling = true)]
        public static extern TG_STARTRESULT StartHighPrecisionTickGenerator_Winmm(int interval, TimerCallback_Winmm callback, out IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        public static extern TG_STARTRESULT StartHighPrecisionTickGenerator_Apple(int interval, TimerCallback_Apple callback, out IntPtr info);

        [DllImport(LibraryName, ExactSpelling = true)]
        public static extern TG_STOPRESULT StopHighPrecisionTickGenerator(IntPtr info);

        #endregion

        #region Methods

        public override API_TYPE Api_GetApiType()
        {
            return GetApiType();
        }

        public override TG_STARTRESULT Api_StartHighPrecisionTickGenerator_Winmm(int interval, TimerCallback_Winmm callback, out IntPtr info)
        {
            return StartHighPrecisionTickGenerator_Winmm(interval, callback, out info);
        }

        public override TG_STARTRESULT Api_StartHighPrecisionTickGenerator_Apple(int interval, TimerCallback_Apple callback, out IntPtr info)
        {
            return StartHighPrecisionTickGenerator_Apple(interval, callback, out info);
        }

        public override TG_STOPRESULT Api_StopHighPrecisionTickGenerator(IntPtr info)
        {
            return StopHighPrecisionTickGenerator(info);
        }

        #endregion
    }
}
