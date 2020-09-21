using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal static class MidiTimerWinApi
    {
        #region Types

        [StructLayout(LayoutKind.Sequential)]
        public struct TIMECAPS
        {
            public uint wPeriodMin;
            public uint wPeriodMax;
        }

        public delegate void TimeProc(uint uID, uint uMsg, uint dwUser, uint dw1, uint dw2);

        #endregion

        #region Constants

        public const uint TIME_ONESHOT = 0;
        public const uint TIME_PERIODIC = 1;

        #endregion

        #region Methods

        [DllImport("winmm.dll", SetLastError = true, ExactSpelling = true)]
        public static extern uint timeGetDevCaps(ref TIMECAPS timeCaps, uint sizeTimeCaps);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint timeBeginPeriod(uint uPeriod);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint timeEndPeriod(uint uPeriod);

        [DllImport("winmm.dll", SetLastError = true, ExactSpelling = true)]
        public static extern uint timeSetEvent(uint uDelay, uint uResolution, TimeProc lpTimeProc, IntPtr dwUser, uint fuEvent);

        [DllImport("winmm.dll", ExactSpelling = true)]
        public static extern uint timeKillEvent(uint uTimerID);

        #endregion
    }
}
