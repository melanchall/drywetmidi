namespace Melanchall.DryWetMidi.Smf
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

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="sequenceNumberEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SequenceNumberEvent sequenceNumberEvent)
        {
            return Equals(sequenceNumberEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="sequenceNumberEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SequenceNumberEvent sequenceNumberEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, sequenceNumberEvent))
                return false;

            if (ReferenceEquals(this, sequenceNumberEvent))
                return true;

            return base.Equals(sequenceNumberEvent, respectDeltaTime) && Number == sequenceNumberEvent.Number;
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
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize()
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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SequenceNumberEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Number.GetHashCode();
        }

        #endregion
    }
}
