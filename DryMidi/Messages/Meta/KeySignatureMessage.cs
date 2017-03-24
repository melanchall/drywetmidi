namespace Melanchall.DryMidi
{
    public sealed class KeySignatureMessage : MetaMessage
    {
        #region Constructor

        public KeySignatureMessage()
        {
        }

        public KeySignatureMessage(sbyte key, byte scale)
            : this()
        {
            Key = key;
            Scale = scale;
        }

        #endregion

        #region Properties

        public sbyte Key { get; set; }

        public byte Scale { get; set; }

        #endregion

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            Key = reader.ReadSByte();
            Scale = reader.ReadByte();
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteSByte(Key);
            writer.WriteByte(Scale);
        }

        internal override int GetContentSize()
        {
            return 2;
        }

        protected override Message CloneMessage()
        {
            return new KeySignatureMessage(Key, Scale);
        }

        public override string ToString()
        {
            return $"Key Signature (key = {Key}, scale = {Scale})";
        }

        #endregion
    }
}
