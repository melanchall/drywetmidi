namespace Melanchall.DryWetMidi
{
    public sealed class KeySignatureEvent : MetaEvent
    {
        #region Constants

        public const sbyte DefaultKey = 0;
        public const byte DefaultScale = 0;

        #endregion

        #region Constructor

        public KeySignatureEvent()
        {
        }

        public KeySignatureEvent(sbyte key, byte scale)
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

        public bool Equals(KeySignatureEvent keySignatureEvent)
        {
            return Equals(keySignatureEvent, true);
        }

        public bool Equals(KeySignatureEvent keySignatureEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, keySignatureEvent))
                return false;

            if (ReferenceEquals(this, keySignatureEvent))
                return true;

            return base.Equals(keySignatureEvent, respectDeltaTime) &&
                   Key == keySignatureEvent.Key &&
                   Scale == keySignatureEvent.Scale;
        }

        #endregion

        #region Overrides

        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            Key = reader.ReadSByte();
            Scale = reader.ReadByte();
        }

        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteSByte(Key);
            writer.WriteByte(Scale);
        }

        protected override int GetContentSize()
        {
            return 2;
        }

        protected override MidiEvent CloneEvent()
        {
            return new KeySignatureEvent(Key, Scale);
        }

        public override string ToString()
        {
            return $"Key Signature (key = {Key}, scale = {Scale})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as KeySignatureEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Key.GetHashCode() ^ Scale.GetHashCode();
        }

        #endregion
    }
}
