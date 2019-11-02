namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a MIDI Port meta event.
    /// </summary>
    /// <remarks>
    /// This optional event specifies the MIDI output port on which data within a track chunk
    /// will be transmitted.
    /// </remarks>
    public sealed class PortPrefixEvent : MetaEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PortPrefixEvent"/>.
        /// </summary>
        public PortPrefixEvent()
            : base(MidiEventType.PortPrefix)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortPrefixEvent"/> with the
        /// specified port.
        /// </summary>
        /// <param name="port">MIDI port.</param>
        public PortPrefixEvent(byte port)
            : this()
        {
            Port = port;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets MIDI port.
        /// </summary>
        public byte Port { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI meta event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size >= 1)
                Port = reader.ReadByte();
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Port);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize(WritingSettings settings)
        {
            return 1;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new PortPrefixEvent(Port);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Port Prefix ({Port})";
        }

        #endregion
    }
}
