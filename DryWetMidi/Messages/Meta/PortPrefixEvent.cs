using System;

namespace Melanchall.DryMidi
{
    public sealed class PortPrefixEvent : MetaEvent
    {
        #region Constructor

        public PortPrefixEvent()
        {
        }

        public PortPrefixEvent(byte port)
            : this()
        {
            Port = port;
        }

        #endregion

        #region Properties

        public byte Port { get; set; }

        #endregion

        #region Methods

        public bool Equals(PortPrefixEvent portPrefixEvent)
        {
            if (ReferenceEquals(null, portPrefixEvent))
                return false;

            if (ReferenceEquals(this, portPrefixEvent))
                return true;

            return base.Equals(portPrefixEvent) && Port == portPrefixEvent.Port;
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

        protected override MidiEvent CloneEvent()
        {
            return new PortPrefixEvent(Port);
        }

        public override string ToString()
        {
            return $"Port Prefix (port = {Port})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PortPrefixEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Port.GetHashCode();
        }

        #endregion
    }
}
