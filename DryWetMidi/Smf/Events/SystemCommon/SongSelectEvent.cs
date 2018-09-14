using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class SongSelectEvent : SystemCommonEvent
    {
        #region Constructor

        public SongSelectEvent()
        {
        }

        public SongSelectEvent(SevenBitNumber number)
        {
            Number = number;
        }

        #endregion

        #region Properties

        public SevenBitNumber Number { get; set; }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            Number = (SevenBitNumber)reader.ReadByte();
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Number);
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 1;
        }

        protected override MidiEvent CloneEvent()
        {
            return new SongSelectEvent(Number);
        }

        public override string ToString()
        {
            return $"Song Number ({Number})";
        }

        #endregion
    }
}
