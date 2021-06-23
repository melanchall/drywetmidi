using System;

namespace Melanchall.DryWetMidi.Devices
{
    internal abstract class TickGeneratorApi
    {
        #region Nested enums

        public enum TG_STARTRESULT
        {
            WINDOWS_TG_STARTRESULT_OK = 0,
            WINDOWS_TG_STARTRESULT_CANTGETDEVICECAPABILITIES = 1,
            WINDOWS_TG_STARTRESULT_CANTSETTIMERCALLBACK = 2
        }

        public enum TG_STOPRESULT
        {
            WINDOWS_TG_STOPRESULT_OK = 0,
            WINDOWS_TG_STOPRESULT_CANTENDPERIOD = 1,
            WINDOWS_TG_STOPRESULT_CANTKILLEVENT = 2
        }

        public delegate void TimerCallback();

        #endregion

        #region Methods

        public abstract TG_STARTRESULT Api_StartHighPrecisionTickGenerator(int interval, TimerCallback callback, out IntPtr info);

        public abstract TG_STOPRESULT Api_StopHighPrecisionTickGenerator(IntPtr info);

        #endregion
    }
}
