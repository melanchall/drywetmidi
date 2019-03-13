using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class SampleDumpRequestSysExData : NonRealTimeSysExData
    {
        #region Properties

        public ushort SampleNumber { get; set; }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, SysExDataReadingSettings settings)
        {
            var sampleNumberLsb = ToSevenBitNumber(reader.ReadByte(), settings, "sample number LSB");
            var sampleNumberMsb = ToSevenBitNumber(reader.ReadByte(), settings, "sample number MSB");
            SampleNumber = DataTypesUtilities.Combine(sampleNumberMsb, sampleNumberLsb);
        }

        public override string ToString()
        {
            return "Sample Dump Request";
        }

        #endregion
    }
}
