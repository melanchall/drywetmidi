namespace Melanchall.DryMidi
{
    public sealed class ChannelPrefixMessage : MetaMessage
    {
        #region Constructor

        public ChannelPrefixMessage()
        {
        }

        public ChannelPrefixMessage(byte channel)
            : this()
        {
            Channel = channel;
        }

        #endregion

        #region Properties

        public byte Channel { get; set; }

        #endregion

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            Channel = reader.ReadByte();
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Channel);
        }

        internal override int GetContentSize()
        {
            return 1;
        }

        protected override Message CloneMessage()
        {
            return new ChannelPrefixMessage(Channel);
        }

        public override string ToString()
        {
            return $"Channel Prefix Message (channel = {Channel})";
        }

        #endregion
    }
}
