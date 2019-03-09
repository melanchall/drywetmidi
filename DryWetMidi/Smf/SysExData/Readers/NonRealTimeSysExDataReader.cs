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
            [UniversalSysExDataIds.NonRealTime.SampleDumpCancel] = typeof(SampleDumpCancelSysExData),
            [UniversalSysExDataIds.NonRealTime.SampleDumpWait] = typeof(SampleDumpWaitSysExData),
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
