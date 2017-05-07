namespace Melanchall.DryWetMidi
{
    public sealed class ChannelPrefixEvent : MetaEvent
    {
        #region Constructor

        public ChannelPrefixEvent()
        {
        }

        public ChannelPrefixEvent(byte channel)
            : this()
        {
            Channel = channel;
        }

        #endregion

        #region Properties

        public byte Channel { get; set; }

        #endregion

        #region Methods

        public bool Equals(ChannelPrefixEvent channelPrefixEvent)
        {
            return Equals(channelPrefixEvent, true);
        }

        public bool Equals(ChannelPrefixEvent channelPrefixEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, channelPrefixEvent))
                return false;

            if (ReferenceEquals(this, channelPrefixEvent))
                return true;

            return base.Equals(channelPrefixEvent, respectDeltaTime) && Channel == channelPrefixEvent.Channel;
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

        protected override MidiEvent CloneEvent()
        {
            return new ChannelPrefixEvent(Channel);
        }

        public override string ToString()
        {
            return $"Channel Prefix (channel = {Channel})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ChannelPrefixEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Channel.GetHashCode();
        }

        #endregion
    }
}
