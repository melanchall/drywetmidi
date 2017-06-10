using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Set Tempo meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI set tempo meta message sets the tempo of a MIDI sequence in terms
    /// of microseconds per quarter note.
    /// </remarks>
    public sealed class SetTempoEvent : MetaEvent
    {
        #region Constants

        /// <summary>
        /// Default tempo.
        /// </summary>
        public const long DefaultTempo = 500000;

        #endregion

        #region Fields

        private long _microsecondsPerBeat = DefaultTempo;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SetTempoEvent"/>.
        /// </summary>
        public SetTempoEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetTempoEvent"/> with the
        /// specified number of microseconds per quarter note.
        /// </summary>
        /// <param name="microsecondsPerQuarterNote">Number of microseconds per quarter note.</param>
        public SetTempoEvent(long microsecondsPerQuarterNote)
            : this()
        {
            MicrosecondsPerQuarterNote = microsecondsPerQuarterNote;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets number of microseconds per quarter note.
        /// </summary>
        public long MicrosecondsPerQuarterNote
        {
            get { return _microsecondsPerBeat; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Value of microseconds per quarter note is negative.",
                                                          value,
                                                          nameof(value));

                _microsecondsPerBeat = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="setTempoEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SetTempoEvent setTempoEvent)
        {
            return Equals(setTempoEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="setTempoEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SetTempoEvent setTempoEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, setTempoEvent))
                return false;

            if (ReferenceEquals(this, setTempoEvent))
                return true;

            return base.Equals(setTempoEvent, respectDeltaTime) &&
                   MicrosecondsPerQuarterNote == setTempoEvent.MicrosecondsPerQuarterNote;
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
            MicrosecondsPerQuarterNote = reader.Read3ByteDword();
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.Write3ByteDword((uint)MicrosecondsPerQuarterNote);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize()
        {
            return 3;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new SetTempoEvent(MicrosecondsPerQuarterNote);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Set Tempo ({MicrosecondsPerQuarterNote})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SetTempoEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ MicrosecondsPerQuarterNote.GetHashCode();
        }

        #endregion
    }
}
