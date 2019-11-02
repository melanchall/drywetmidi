namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a MIDI Channel Prefix meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI channel prefix meta message specifies a MIDI channel so that meta messages that
    /// follow are specific to a channel.
    /// </remarks>
    public sealed class ChannelPrefixEvent : MetaEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelPrefixEvent"/>.
        /// </summary>
        public ChannelPrefixEvent()
            : base(MidiEventType.ChannelPrefix)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelPrefixEvent"/> with the
        /// specified MIDI channel.
        /// </summary>
        /// <param name="channel">MIDI channel.</param>
        public ChannelPrefixEvent(byte channel)
            : this()
        {
            Channel = channel;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets MIDI channel.
        /// </summary>
        public byte Channel { get; set; }

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
            Channel = reader.ReadByte();
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Channel);
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
            return new ChannelPrefixEvent(Channel);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Channel Prefix ({Channel})";
        }

        #endregion
    }
}
