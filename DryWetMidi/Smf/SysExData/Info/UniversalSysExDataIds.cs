namespace Melanchall.DryWetMidi.Smf
{
    internal static class UniversalSysExDataIds
    {
        internal static class NonRealTime
        {
            public static readonly UniversalSysExDataId SampleDumpHeader = new UniversalSysExDataId(0x01);
            public static readonly UniversalSysExDataId SampleDumpDataPacket = new UniversalSysExDataId(0x02);
            public static readonly UniversalSysExDataId SampleDumpRequest = new UniversalSysExDataId(0x03);

            public static readonly UniversalSysExDataId Ack = new UniversalSysExDataId(0x7F);
            public static readonly UniversalSysExDataId Nak = new UniversalSysExDataId(0x7E);
            public static readonly UniversalSysExDataId Cancel = new UniversalSysExDataId(0x7D);
            public static readonly UniversalSysExDataId Wait = new UniversalSysExDataId(0x7C);
            public static readonly UniversalSysExDataId Eof = new UniversalSysExDataId(0x7B);

            public static readonly UniversalSysExDataId MtcSpecial = new UniversalSysExDataId(0x04, 0x00);
            public static readonly UniversalSysExDataId MtcPunchIn = new UniversalSysExDataId(0x04, 0x01);
            public static readonly UniversalSysExDataId MtcPunchOut = new UniversalSysExDataId(0x04, 0x02);
            public static readonly UniversalSysExDataId MtcDeletePunchIn = new UniversalSysExDataId(0x04, 0x03);
            public static readonly UniversalSysExDataId MtcDeletePunchOut = new UniversalSysExDataId(0x04, 0x04);
            public static readonly UniversalSysExDataId MtcEventStart = new UniversalSysExDataId(0x04, 0x05);
            public static readonly UniversalSysExDataId MtcEventStop = new UniversalSysExDataId(0x04, 0x06);
            public static readonly UniversalSysExDataId MtcEventStartWithInfo = new UniversalSysExDataId(0x04, 0x07);
            public static readonly UniversalSysExDataId MtcEventStopWithInfo = new UniversalSysExDataId(0x04, 0x08);
            public static readonly UniversalSysExDataId MtcDeleteEventStart = new UniversalSysExDataId(0x04, 0x09);
            public static readonly UniversalSysExDataId MtcDeleteEventStop = new UniversalSysExDataId(0x04, 0x0A);
            public static readonly UniversalSysExDataId MtcCuePoint = new UniversalSysExDataId(0x04, 0x0B);
            public static readonly UniversalSysExDataId MtcCuePointWithInfo = new UniversalSysExDataId(0x04, 0x0C);
            public static readonly UniversalSysExDataId MtcDeleteCuePoint = new UniversalSysExDataId(0x04, 0x0D);
            public static readonly UniversalSysExDataId MtcEventNameInInfo = new UniversalSysExDataId(0x04, 0x0E);
        }

        internal static class RealTime
        {
        }
    }
}
