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

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            Port = reader.ReadByte();
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Port);
        }

        internal override int GetContentSize()
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
