namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Sequence/Track Name meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI track name meta message defines either the name of a MIDI sequence
    /// (when in MIDI type 0 or MIDI type 2 files, or when in the first track of a MIDI type 1 file),
    /// or the name of a MIDI track (when in other tracks of a MIDI type 1 file).
    /// </remarks>
    public sealed class SequenceTrackNameEvent : BaseTextEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceTrackNameEvent"/>.
        /// </summary>
        public SequenceTrackNameEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceTrackNameEvent"/> with the
        /// specified sequence or track name.
        /// </summary>
        /// <param name="name">Name of a sequence or track.</param>
        public SequenceTrackNameEvent(string name)
            : base(name)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="sequenceTrackNameEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SequenceTrackNameEvent sequenceTrackNameEvent)
        {
            return Equals(sequenceTrackNameEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="sequenceTrackNameEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(SequenceTrackNameEvent sequenceTrackNameEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, sequenceTrackNameEvent))
                return false;

            if (ReferenceEquals(this, sequenceTrackNameEvent))
                return true;

            return base.Equals(sequenceTrackNameEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new SequenceTrackNameEvent(Text);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Sequence/Track Name ({Text})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SequenceTrackNameEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
