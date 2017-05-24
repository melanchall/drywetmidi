namespace Melanchall.DryWetMidi.Smf
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

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="portPrefixEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(PortPrefixEvent portPrefixEvent)
        {
            return Equals(portPrefixEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="portPrefixEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
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
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize()
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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as PortPrefixEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Port.GetHashCode();
        }

        #endregion
    }
}
