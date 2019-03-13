using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class NonRealTimeSysExDataReader : UniversalSysExDataReader
    {
        #region Constants

        private static readonly Dictionary<UniversalSysExDataId, Type> SysExDataTypes = new Dictionary<UniversalSysExDataId, Type>
        {
            [UniversalSysExDataIds.NonRealTime.SampleDumpHeader] = typeof(SampleDumpHeaderSysExData),
            [UniversalSysExDataIds.NonRealTime.SampleDumpDataPacket] = typeof(SampleDumpDataPacketSysExData),
            [UniversalSysExDataIds.NonRealTime.SampleDumpRequest] = typeof(SampleDumpRequestSysExData),

            [UniversalSysExDataIds.NonRealTime.Ack] = typeof(AckSysExData),
            [UniversalSysExDataIds.NonRealTime.Nak] = typeof(NakSysExData),
            [UniversalSysExDataIds.NonRealTime.Cancel] = typeof(CancelSysExData),
            [UniversalSysExDataIds.NonRealTime.Wait] = typeof(WaitSysExData),
            [UniversalSysExDataIds.NonRealTime.Eof] = typeof(EofSysExData),

            [UniversalSysExDataIds.NonRealTime.MtcCuePoint] = typeof(MtcCuePointSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcCuePointWithInfo] = typeof(MtcCuePointWithInfoSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcDeleteCuePoint] = typeof(MtcDeleteCuePointSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcDeleteEventStart] = typeof(MtcDeleteEventStartSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcDeleteEventStop] = typeof(MtcDeleteEventStopSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcDeletePunchIn] = typeof(MtcDeletePunchInSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcDeletePunchOut] = typeof(MtcDeletePunchOutSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcEventNameInInfo] = typeof(MtcEventNameInInfoSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcEventStart] = typeof(MtcEventStartSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcEventStartWithInfo] = typeof(MtcEventStartWithInfoSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcEventStop] = typeof(MtcEventStopSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcEventStopWithInfo] = typeof(MtcEventStopWithInfoSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcPunchIn] = typeof(MtcPunchInSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcPunchOut] = typeof(MtcPunchOutSysExData),
            [UniversalSysExDataIds.NonRealTime.MtcSpecial] = typeof(MtcSpecialSysExData),
        };

        #endregion

        #region Constructor

        public NonRealTimeSysExDataReader()
            : base(SysExDataTypes)
        {
        }

        #endregion
    }
}
