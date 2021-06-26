using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class TickGeneratorApi32 : TickGeneratorApi
    {
        #region Extern functions

        [DllImport("Melanchall_DryWetMidi_Native32", ExactSpelling = true)]
        private static extern API_TYPE GetApiType();

        [DllImport("Melanchall_DryWetMidi_Native32", ExactSpelling = true)]
        private static extern TG_STARTRESULT StartHighPrecisionTickGenerator_Winmm(int interval, TimerCallback_Winmm callback, out IntPtr info);

        [DllImport("Melanchall_DryWetMidi_Native32", ExactSpelling = true)]
        private static extern TG_STARTRESULT StartHighPrecisionTickGenerator_Apple(int interval, TimerCallback_Apple callback, out IntPtr info);

        [DllImport("Melanchall_DryWetMidi_Native32", ExactSpelling = true)]
        private static extern TG_STOPRESULT StopHighPrecisionTickGenerator(IntPtr info);

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
