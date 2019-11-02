namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Copyright Notice meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI copyright notice meta message places a copyright notice in a MIDI file.
    /// </remarks>
    public sealed class CopyrightNoticeEvent : BaseTextEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyrightNoticeEvent"/>.
        /// </summary>
        public CopyrightNoticeEvent()
            : base(MidiEventType.CopyrightNotice)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyrightNoticeEvent"/> with the
        /// specified text of copyright notice.
        /// </summary>
        /// <param name="text">Text of copyright notice.</param>
        public CopyrightNoticeEvent(string text)
            : base(MidiEventType.CopyrightNotice, text)
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
            return new CopyrightNoticeEvent(Text);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Copyright Notice ({Text})";
        }

        #endregion
    }
}
