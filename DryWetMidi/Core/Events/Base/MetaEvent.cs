namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a MIDI file meta event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Meta event specifies non-MIDI information useful to specific applications.
    /// </para>
    /// <para>
    /// As with <see cref="MidiChunk">chunks</see>, future meta events may be designed which may not be known to
    /// existing programs, so programs must properly ignore meta events which they do not recognize, and indeed,
    /// should expect to see them. Programs must never ignore the length of a meta-event which they do recognize,
    /// and they shouldn't be surprised if it's bigger than they expected. If so, they must ignore everything past
    /// what they know about. However, they must not add anything of their own to the end of a meta-event.
    /// </para>
    /// <para>
    /// You can create custom meta events that can be read from and written to a MIDI file.
    /// See <see href="xref:a_custom_meta_event">Custom meta events</see> article to learn more.
    /// </para>
    /// <para>
    /// See <see href="https://midi.org/standard-midi-files-specification"/> for detailed MIDI file specification.
    /// </para>
    /// </remarks>
    public abstract class MetaEvent : MidiEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaEvent"/>.
        /// </summary>
        protected MetaEvent()
            : this(MidiEventType.CustomMeta)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaEvent"/> with the specified event type.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        internal MetaEvent(MidiEventType eventType)
            : base(eventType)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        internal sealed override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            ReadContent(reader, settings, size);
        }

        /// <summary>
        /// Writes content of a MIDI event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        internal sealed override void Write(MidiWriter writer, WritingSettings settings)
        {
            WriteContent(writer, settings);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        internal sealed override int GetSize(WritingSettings settings)
        {
            return GetContentSize(settings);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns array of status bytes of standard meta events.
        /// </summary>
        /// <returns>Array of status bytes of standard meta events.</returns>
        /// <remarks>
        /// This method can be used for choosing status byte for custom meta event to prevent
        /// the status byte is equal to one of the standard ones.
        /// </remarks>
        public static byte[] GetStandardMetaEventStatusBytes()
        {
            return StandardMetaEventStatusBytes.GetStatusBytes();
        }

        /// <summary>
        /// Reads content of a MIDI meta event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        protected abstract void ReadContent(MidiReader reader, ReadingSettings settings, int size);

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected abstract void WriteContent(MidiWriter writer, WritingSettings settings);

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        protected abstract int GetContentSize(WritingSettings settings);

        #endregion
    }
}
