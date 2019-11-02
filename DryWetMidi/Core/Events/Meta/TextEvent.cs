namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Text meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI text meta message defines some text to be carried within a MIDI file.
    /// </remarks>
    public sealed class TextEvent : BaseTextEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEvent"/>.
        /// </summary>
        public TextEvent()
            : base(MidiEventType.Text)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEvent"/> with the
        /// specified text.
        /// </summary>
        /// <param name="text">Text of the message.</param>
        public TextEvent(string text)
            : base(MidiEventType.Text, text)
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
            return new TextEvent(Text);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Text ({Text})";
        }

        #endregion
    }
}
