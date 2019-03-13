using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class SampleDumpDataPacketSysExData : NonRealTimeSysExData
    {
        #region Constants

        private const int DataLength = 120;

        #endregion

        #region Properties

        public SevenBitNumber PacketNumber { get; set; }

        public SevenBitNumber[] SampleData { get; set; }

        public SevenBitNumber Checksum { get; set; }

        public bool IsValid
        {
            get
            {
                var actualChecksum = UniversalSysExDataStatusBytes.NonRealTime ^
                                     DeviceId ^
                                     UniversalSysExDataIds.NonRealTime.SampleDumpDataPacket.SubId1 ^
                                     PacketNumber ^
                                     SampleData?.Aggregate(0, (result, d) => result ^ d);
                actualChecksum >>= 1;

                return actualChecksum == Checksum;
            }
        }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, SysExDataReadingSettings settings)
        {
            PacketNumber = ToSevenBitNumber(reader.ReadByte(), settings, "packet number");

            var data = reader.ReadBytes(DataLength);
            if (data.Length < DataLength)
                throw new NotEnoughBytesException("Not enough bytes in the stream to read packet data.", DataLength, data.Length);

            SampleData = ToSevenBitNumbers(data, settings, "sample data", leftJustified: true);

            Checksum = ToSevenBitNumber(reader.ReadByte(), settings, "checksum", leftJustified: true);
        }

        public override string ToString()
        {
            return "Sample Dump Data Packet";
        }

        #endregion
    }
}
