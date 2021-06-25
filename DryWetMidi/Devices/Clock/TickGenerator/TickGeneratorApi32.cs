using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class TickGeneratorApi32 : TickGeneratorApi
    {
        #region Extern functions

        [DllImport("Melanchall_DryWetMidi_Native32", ExactSpelling = true)]
        public static extern TG_STARTRESULT StartHighPrecisionTickGenerator(int interval, TimerCallback callback, out IntPtr info);

        [DllImport("Melanchall_DryWetMidi_Native32", ExactSpelling = true)]
        public static extern TG_STOPRESULT StopHighPrecisionTickGenerator(IntPtr info);

        #endregion

        #region Methods

        public override TG_STARTRESULT Api_StartHighPrecisionTickGenerator(int interval, TimerCallback callback, out IntPtr info)
        {
            return StartHighPrecisionTickGenerator(interval, callback, out info);
        }

        public override TG_STOPRESULT Api_StopHighPrecisionTickGenerator(IntPtr info)
        {
            return StopHighPrecisionTickGenerator(info);
        }

        #endregion
    }
}
