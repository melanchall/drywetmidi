namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Sequence Number meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI sequence number meta message defines the number of a sequence in type 0 and 1 MIDI files,
    /// or the pattern number in type 2 MIDI files.
    /// </remarks>
    public sealed class SequenceNumberEvent : MetaEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceNumberEvent"/>.
        /// </summary>
        public SequenceNumberEvent()
            : base(MidiEventType.SequenceNumber)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceNumberEvent"/> with the
        /// specified number of a sequence.
        /// </summary>
        /// <param name="number">The number of a sequence.</param>
        public SequenceNumberEvent(ushort number)
            : this()
        {
            Number = number;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the number of a sequence.
        /// </summary>
        public ushort Number { get; set; }

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
            // A shortened version can be used in format 2 MIDI files : the 2 data bytes can be omitted
            // (thus length must be 0), whereupon the sequence number is derived from the track chunk's
            // position within the file.
            if (size < 2)
                return;

            Number = reader.ReadWord();
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteWord(Number);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize(WritingSettings settings)
        {
            return 2;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new SequenceNumberEvent(Number);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Sequence Number ({Number})";
        }

        #endregion
    }
}
