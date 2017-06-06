namespace Melanchall.DryWetMidi.Smf
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
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEvent"/> with the
        /// specified text.
        /// </summary>
        /// <param name="text">Text of the message.</param>
        public TextEvent(string text)
            : base(text)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="textEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(TextEvent textEvent)
        {
            return Equals(textEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="textEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(TextEvent textEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, textEvent))
                return false;

            if (ReferenceEquals(this, textEvent))
                return true;

            return base.Equals(textEvent, respectDeltaTime);
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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TextEvent);
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
