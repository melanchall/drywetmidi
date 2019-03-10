using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public abstract class HandshakingSysExData : NonRealTimeSysExData
    {
        #region Properties

        public SevenBitNumber PacketNumber { get; set; }

        #endregion

        #region Overrides

        internal sealed override void Read(MidiReader reader, SysExDataReadingSettings settings)
        {
            PacketNumber = ToSevenBitNumber(reader.ReadByte(), settings, "packet number");
        }

        #endregion
    }
}
