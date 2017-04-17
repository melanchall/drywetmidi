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

        #region Methods

        public bool Equals(KeySignatureMessage keySignatureMessage)
        {
            if (ReferenceEquals(null, keySignatureMessage))
                return false;

            if (ReferenceEquals(this, keySignatureMessage))
                return true;

            return base.Equals(keySignatureMessage) &&
                   Key == keySignatureMessage.Key &&
                   Scale == keySignatureMessage.Scale;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            Key = reader.ReadSByte();
            Scale = reader.ReadByte();
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteSByte(Key);
            writer.WriteByte(Scale);
        }

        protected override int GetContentDataSize()
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

        public override bool Equals(object obj)
        {
            return Equals(obj as KeySignatureMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Key.GetHashCode() ^ Scale.GetHashCode();
        }

        #endregion
    }
}
