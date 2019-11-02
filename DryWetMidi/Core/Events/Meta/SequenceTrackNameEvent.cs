namespace Melanchall.DryWetMidi.Core
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
            : base(MidiEventType.SequenceTrackName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceTrackNameEvent"/> with the
        /// specified sequence or track name.
        /// </summary>
        /// <param name="name">Name of a sequence or track.</param>
        public SequenceTrackNameEvent(string name)
            : base(MidiEventType.SequenceTrackName, name)
        {
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

        #endregion
    }
}
