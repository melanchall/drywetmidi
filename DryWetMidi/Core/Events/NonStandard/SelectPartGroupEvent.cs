namespace Melanchall.DryWetMidi.Core
{
    public sealed class SelectPartGroupEvent : NonStandardEvent
    {
        #region Constants

        private const byte DefaultPartGroupNumber = 0;

        #endregion

        #region Constructor

        public SelectPartGroupEvent()
            : this(DefaultPartGroupNumber)
        {
        }

        public SelectPartGroupEvent(byte partGroupNumber)
            : base(MidiEventType.SelectPartGroup)
        {
            PartGroupNumber = partGroupNumber;
        }

        #endregion

        #region Properties

        public byte PartGroupNumber { get; set; }

        #endregion

        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new SelectPartGroupEvent(PartGroupNumber);
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 1;
        }

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            PartGroupNumber = reader.ReadByte();
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(PartGroupNumber);
        }

        #endregion
    }
}
