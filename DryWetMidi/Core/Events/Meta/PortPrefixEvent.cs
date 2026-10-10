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

        /// <inheritdoc/>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size >= 1)
                Port = reader.ReadByte();
        }

        /// <inheritdoc/>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Port);
        }

        /// <inheritdoc/>
        protected override int GetContentSize(WritingSettings settings)
        {
            return 1;
        }

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new PortPrefixEvent(Port);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Port Prefix ({Port})";
        }

        #endregion
    }
}
