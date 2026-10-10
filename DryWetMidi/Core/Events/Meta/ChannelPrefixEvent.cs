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
        /// Gets or sets a MIDI channel.
        /// </summary>
        public byte Channel { get; set; }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            Channel = reader.ReadByte();
        }

        /// <inheritdoc/>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Channel);
        }

        /// <inheritdoc/>
        protected override int GetContentSize(WritingSettings settings)
        {
            return 1;
        }

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new ChannelPrefixEvent(Channel);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Channel Prefix ({Channel})";
        }

        #endregion
    }
}
