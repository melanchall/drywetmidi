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

        #region Methods

        public bool Equals(ChannelPrefixMessage channelPrefixMessage)
        {
            if (ReferenceEquals(null, channelPrefixMessage))
                return false;

            if (ReferenceEquals(this, channelPrefixMessage))
                return true;

            return base.Equals(channelPrefixMessage) && Channel == channelPrefixMessage.Channel;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            Channel = reader.ReadByte();
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Channel);
        }

        protected override int GetContentDataSize()
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

        public override bool Equals(object obj)
        {
            return Equals(obj as ChannelPrefixMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Channel.GetHashCode();
        }

        #endregion
    }
}
