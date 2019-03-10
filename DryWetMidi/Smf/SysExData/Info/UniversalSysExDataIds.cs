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
        }

        internal static class RealTime
        {
        }
    }
}
