using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class SongPositionPointerEvent : SystemCommonEvent
    {
        #region Constructor

        public SongPositionPointerEvent()
        {
        }

        public SongPositionPointerEvent(SevenBitNumber msb, SevenBitNumber lsb)
        {
            Msb = msb;
            Lsb = lsb;
        }

        #endregion

        #region Properties

        public SevenBitNumber Msb { get; set; }

        public SevenBitNumber Lsb { get; set; }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            // TODO: apply policy
            Lsb = (SevenBitNumber)reader.ReadByte();
            Msb = (SevenBitNumber)reader.ReadByte();
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Lsb);
            writer.WriteByte(Msb);
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 2;
        }

        protected override MidiEvent CloneEvent()
        {
            return new SongPositionPointerEvent(Msb, Lsb);
        }

        public override string ToString()
        {
            return $"Song Position Pointer ({Msb}, {Lsb})";
        }

        #endregion
    }
}
