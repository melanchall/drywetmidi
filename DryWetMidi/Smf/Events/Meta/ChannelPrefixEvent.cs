namespace Melanchall.DryWetMidi.Smf
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

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="channelPrefixEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(ChannelPrefixEvent channelPrefixEvent)
        {
            return Equals(channelPrefixEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="channelPrefixEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ChannelPrefixEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Channel.GetHashCode();
        }

        #endregion
    }
}
