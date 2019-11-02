namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Lyric meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI lyrics meta message shows the lyrics of a song at a particular time in the MIDI sequence.
    /// </remarks>
    public sealed class LyricEvent : BaseTextEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LyricEvent"/>.
        /// </summary>
        public LyricEvent()
            : base(MidiEventType.Lyric)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LyricEvent"/> with the
        /// specified text of lyrics.
        /// </summary>
        /// <param name="text">Text of the lyrics.</param>
        public LyricEvent(string text)
            : base(MidiEventType.Lyric, text)
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
            return new LyricEvent(Text);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Lyric ({Text})";
        }

        #endregion
    }
}
