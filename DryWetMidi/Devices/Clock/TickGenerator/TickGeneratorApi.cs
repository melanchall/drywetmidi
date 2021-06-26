using System;

namespace Melanchall.DryWetMidi.Devices
{
    internal abstract class TickGeneratorApi
    {
        #region Nested enums

        public enum API_TYPE
        {
            API_TYPE_WINMM = 0,
            API_TYPE_APPLE = 1
        }

        public enum TG_STARTRESULT
        {
            TG_STARTRESULT_OK = 0,

            TG_STARTRESULT_CANTGETDEVICECAPABILITIES = 1,
            TG_STARTRESULT_CANTSETTIMERCALLBACK = 2,

            TG_STARTRESULT_NORESOURCES = 101,
            TG_STARTRESULT_BADTHREADATTRIBUTE = 102,
            TG_STARTRESULT_UNKNOWNERROR = 199
        }

        public enum TG_STOPRESULT
        {
            TG_STOPRESULT_OK = 0,

            TG_STOPRESULT_CANTENDPERIOD = 1,
            TG_STOPRESULT_CANTKILLEVENT = 2
        }

        #endregion

        #region Delegates

        public delegate void TimerCallback_Winmm(uint uID, uint uMsg, uint dwUser, uint dw1, uint dw2);

        public delegate void TimerCallback_Apple(IntPtr timer, IntPtr info);

        #endregion

        #region Methods

        public abstract API_TYPE Api_GetApiType();

        public abstract TG_STARTRESULT Api_StartHighPrecisionTickGenerator_Winmm(int interval, TimerCallback_Winmm callback, out IntPtr info);

        public abstract TG_STARTRESULT Api_StartHighPrecisionTickGenerator_Apple(int interval, TimerCallback_Apple callback, out IntPtr info);

        public abstract TG_STOPRESULT Api_StopHighPrecisionTickGenerator(IntPtr info);

        #endregion
    }
}
