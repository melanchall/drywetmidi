namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Marker meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI marker meta message marks a point in time for a MIDI sequence.
    /// </remarks>
    public sealed class MarkerEvent : BaseTextEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkerEvent"/>.
        /// </summary>
        public MarkerEvent()
            : base(MidiEventType.Marker)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkerEvent"/> with the
        /// specified text of marker.
        /// </summary>
        /// <param name="text">Text of the marker.</param>
        public MarkerEvent(string text)
            : base(MidiEventType.Marker, text)
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
            return new MarkerEvent(Text);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Marker ({Text})";
        }

        #endregion
    }
}
