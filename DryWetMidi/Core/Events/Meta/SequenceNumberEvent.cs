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

        /// <inheritdoc/>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            // A shortened version can be used in format 2 MIDI files : the 2 data bytes can be omitted
            // (thus length must be 0), whereupon the sequence number is derived from the track chunk's
            // position within the file.
            if (size < 2)
                return;

            Number = reader.ReadWord();
        }

        /// <inheritdoc/>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteWord(Number);
        }

        /// <inheritdoc/>
        protected override int GetContentSize(WritingSettings settings)
        {
            return 2;
        }

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new SequenceNumberEvent(Number);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Sequence Number ({Number})";
        }

        #endregion
    }
}
