using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Sequencer Specific meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI sequencer specific meta message carries information that is specific to a
    /// MIDI sequencer produced by a certain MIDI manufacturer.
    /// </remarks>
    public sealed class SequencerSpecificEvent : MetaEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencerSpecificEvent"/>.
        /// </summary>
        public SequencerSpecificEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencerSpecificEvent"/> with the
        /// specified data.
        /// </summary>
        /// <param name="data">Sequencer specific data.</param>
        public SequencerSpecificEvent(byte[] data)
            : this()
        {
            Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets sequencer specific data.
        /// </summary>
        public byte[] Data { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="sequencerSpecificEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SequencerSpecificEvent sequencerSpecificEvent)
        {
            return Equals(sequencerSpecificEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="sequencerSpecificEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SequencerSpecificEvent sequencerSpecificEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, sequencerSpecificEvent))
                return false;

            if (ReferenceEquals(this, sequencerSpecificEvent))
                return true;

            return base.Equals(sequencerSpecificEvent, respectDeltaTime) &&
                   ArrayUtilities.Equals(Data, sequencerSpecificEvent.Data);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI meta event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        /// <exception cref="ArgumentOutOfRangeException">Sequencer specific event cannot be read since the size is
        /// negative number.</exception>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Sequencer specific event cannot be read since the size is negative number.");

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
            return new SequencerSpecificEvent(Data?.Clone() as byte[]);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "Sequencer Specific";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SequencerSpecificEvent);
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
