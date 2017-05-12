namespace Melanchall.DryWetMidi
{
    public sealed class TimeSignatureEvent : MetaEvent
    {
        #region Constants

        public const byte DefaultNumerator = 4;
        public const byte DefaultDenominator = 2;
        public const byte DefaultClocks = 24;
        public const byte Default32ndNotesPerBeat = 8;

        #endregion

        #region Constructor

        public TimeSignatureEvent()
        {
        }

        public TimeSignatureEvent(byte numerator, byte denominator, byte clocks, byte numberOf32ndNotesPerBeat)
            : this()
        {
            Numerator = numerator;
            Denominator = denominator;
            Clocks = clocks;
            NumberOf32ndNotesPerBeat = numberOf32ndNotesPerBeat;
        }

        #endregion

        #region Properties

        public byte Numerator { get; set; } = DefaultNumerator;

        public byte Denominator { get; set; } = DefaultDenominator;

        public byte Clocks { get; set; } = DefaultClocks;

        public byte NumberOf32ndNotesPerBeat { get; set; } = Default32ndNotesPerBeat;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="timeSignatureEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(TimeSignatureEvent timeSignatureEvent)
        {
            return Equals(timeSignatureEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="timeSignatureEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(TimeSignatureEvent timeSignatureEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, timeSignatureEvent))
                return false;

            if (ReferenceEquals(this, timeSignatureEvent))
                return true;

            return base.Equals(timeSignatureEvent, respectDeltaTime) &&
                   Numerator == timeSignatureEvent.Numerator &&
                   Denominator == timeSignatureEvent.Denominator &&
                   Clocks == timeSignatureEvent.Clocks &&
                   NumberOf32ndNotesPerBeat == timeSignatureEvent.NumberOf32ndNotesPerBeat;
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
            Numerator = reader.ReadByte();
            Denominator = reader.ReadByte();

            if (size >= 4)
            {
                Clocks = reader.ReadByte();
                NumberOf32ndNotesPerBeat = reader.ReadByte();
            }
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Numerator);
            writer.WriteByte(Denominator);
            writer.WriteByte(Clocks);
            writer.WriteByte(NumberOf32ndNotesPerBeat);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize()
        {
            return 4;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new TimeSignatureEvent(Numerator, Denominator, Clocks, NumberOf32ndNotesPerBeat);
        }

        public override string ToString()
        {
            return $"Time Signature (numerator = {Numerator}, denominator = {Denominator}, clocks = {Clocks}, 32nd notes per beat = {NumberOf32ndNotesPerBeat})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TimeSignatureEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Numerator.GetHashCode() ^
                                        Denominator.GetHashCode() ^
                                        Clocks.GetHashCode() ^
                                        NumberOf32ndNotesPerBeat.GetHashCode();
        }

        #endregion
    }
}
