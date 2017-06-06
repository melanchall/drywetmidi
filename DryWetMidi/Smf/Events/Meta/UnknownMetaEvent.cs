using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents an unknown meta event.
    /// </summary>
    /// <remarks>
    /// Structure of meta eventa allows custom ones be implemented and stored within a MIDI file.
    /// Any meta event DryWetMIDI doesn't know about will be read as an instance of the
    /// <see cref="UnknownMetaEvent"/>.
    /// </remarks>
    public sealed class UnknownMetaEvent : MetaEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownMetaEvent"/> with the
        /// specified status byte.
        /// </summary>
        /// <param name="statusByte">Status byte of the meta event.</param>
        internal UnknownMetaEvent(byte statusByte)
            : this(statusByte, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownMetaEvent"/> with the
        /// specified status byte and data.
        /// </summary>
        /// <param name="statusByte">Status byte of the meta event.</param>
        /// <param name="data">Data of an unknown meta event.</param>
        private UnknownMetaEvent(byte statusByte, byte[] data)
        {
            StatusByte = statusByte;
            Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the status byte of the meta event.
        /// </summary>
        public byte StatusByte { get; }

        /// <summary>
        /// Gets the content of the meta event as array of bytes.
        /// </summary>
        public byte[] Data { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="unknownMetaEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(UnknownMetaEvent unknownMetaEvent)
        {
            return Equals(unknownMetaEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="unknownMetaEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(UnknownMetaEvent unknownMetaEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, unknownMetaEvent))
                return false;

            if (ReferenceEquals(this, unknownMetaEvent))
                return true;

            return base.Equals(unknownMetaEvent, respectDeltaTime) &&
                   ArrayUtilities.Equals(Data, unknownMetaEvent.Data);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI meta event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        /// <exception cref="ArgumentOutOfRangeException">Unknown meta event cannot be read since the size is
        /// negative number.</exception>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Unknown meta event cannot be read since the size is negative number.");

            Data = reader.ReadBytes(size);
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize()
        {
            return Data?.Length ?? 0;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new UnknownMetaEvent(StatusByte, Data?.Clone() as byte[]);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Unknown meta event ({StatusByte})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as UnknownMetaEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Data?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
