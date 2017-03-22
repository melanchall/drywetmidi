namespace Melanchall.DryMidi
{
    public sealed class SequenceNumberMessage : MetaMessage
    {
        #region Constructor

        public SequenceNumberMessage()
        {
        }

        public SequenceNumberMessage(short number)
            : this()
        {
            Number = number;
        }

        #endregion

        #region Properties

        public short Number { get; set; }

        #endregion

        #region Overrides

        public override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            Number = reader.ReadInt16();
        }

        public override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteInt16(Number);
        }

        public override int GetContentSize()
        {
            return 2;
        }

        protected override Message CloneMessage()
        {
            return new SequenceNumberMessage(Number);
        }

        public override string ToString()
        {
            return $"Sequence Number (number = {Number})";
        }

        #endregion
    }
}
