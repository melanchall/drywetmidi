using System;

namespace Melanchall.DryWetMidi
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
            return Equals(portPrefixEvent, true);
        }

        public bool Equals(PortPrefixEvent portPrefixEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, portPrefixEvent))
                return false;

            if (ReferenceEquals(this, portPrefixEvent))
                return true;

            return base.Equals(portPrefixEvent, respectDeltaTime) && Port == portPrefixEvent.Port;
        }

        #endregion

        #region Overrides

        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            Port = reader.ReadByte();
        }

        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Port);
        }

        protected override int GetContentSize()
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
