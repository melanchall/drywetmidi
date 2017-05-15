namespace Melanchall.DryWetMidi.Smf
{
    public sealed class SmpteOffsetEvent : MetaEvent
    {
        #region Constructor

        public SmpteOffsetEvent()
        {
        }

        public SmpteOffsetEvent(byte hours, byte minutes, byte seconds, byte frames, byte subFrames)
            : this()
        {
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Frames = frames;
            SubFrames = subFrames;
        }

        #endregion

        #region Properties

        public byte Hours { get; set; }

        public byte Minutes { get; set; }

        public byte Seconds { get; set; }

        public byte Frames { get; set; }

        public byte SubFrames { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="smpteOffsetEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SmpteOffsetEvent smpteOffsetEvent)
        {
            return Equals(smpteOffsetEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="smpteOffsetEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SmpteOffsetEvent smpteOffsetEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, smpteOffsetEvent))
                return false;

            if (ReferenceEquals(this, smpteOffsetEvent))
                return true;

            return base.Equals(smpteOffsetEvent, respectDeltaTime) &&
                   Hours == smpteOffsetEvent.Hours &&
                   Minutes == smpteOffsetEvent.Minutes &&
                   Seconds == smpteOffsetEvent.Seconds &&
                   Frames == smpteOffsetEvent.Frames &&
                   SubFrames == smpteOffsetEvent.SubFrames;
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
            Hours = reader.ReadByte();
            Minutes = reader.ReadByte();
            Seconds = reader.ReadByte();
            Frames = reader.ReadByte();
            SubFrames = reader.ReadByte();
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Hours);
            writer.WriteByte(Minutes);
            writer.WriteByte(Seconds);
            writer.WriteByte(Frames);
            writer.WriteByte(SubFrames);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize()
        {
            return 5;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new SmpteOffsetEvent(Hours, Minutes, Seconds, Frames, SubFrames);
        }

        public override string ToString()
        {
            return $"SMPTE Offset (hours = {Hours}, minutes = {Minutes}, seconds = {Seconds}, frames = {Frames}, subframes = {SubFrames})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SmpteOffsetEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Hours.GetHashCode() ^
                                        Minutes.GetHashCode() ^
                                        Seconds.GetHashCode() ^
                                        Frames.GetHashCode() ^
                                        SubFrames.GetHashCode();
        }

        #endregion
    }
}
