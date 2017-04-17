using System;

namespace Melanchall.DryMidi
{
    public sealed class PortPrefixMessage : MetaMessage
    {
        #region Constructor

        public PortPrefixMessage()
        {
        }

        public PortPrefixMessage(byte port)
            : this()
        {
            Port = port;
        }

        #endregion

        #region Properties

        public byte Port { get; set; }

        #endregion

        #region Methods

        public bool Equals(PortPrefixMessage portPrefixMessage)
        {
            if (ReferenceEquals(null, portPrefixMessage))
                return false;

            if (ReferenceEquals(this, portPrefixMessage))
                return true;

            return base.Equals(portPrefixMessage) && Port == portPrefixMessage.Port;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            Port = reader.ReadByte();
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Port);
        }

        protected override int GetContentDataSize()
        {
            return 1;
        }

        protected override Message CloneMessage()
        {
            return new PortPrefixMessage(Port);
        }

        public override string ToString()
        {
            return $"Port Prefix (port = {Port})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PortPrefixMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Port.GetHashCode();
        }

        #endregion
    }
}
