using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a text meta event.
    /// </summary>
    /// <remarks>
    /// There are several meta events that have text content and the same structure. All these
    /// events are derivde from <see cref="BaseTextEvent"/>.
    /// </remarks>
    public abstract class BaseTextEvent : MetaEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTextEvent"/>.
        /// </summary>
        public BaseTextEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTextEvent"/> with the specified text.
        /// </summary>
        /// <param name="text">Text contained in the event.</param>
        public BaseTextEvent(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets text contained in the event.
        /// </summary>
        public string Text { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="baseTextEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(BaseTextEvent baseTextEvent)
        {
            return Equals(baseTextEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="baseTextEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(BaseTextEvent baseTextEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, baseTextEvent))
                return false;

            if (ReferenceEquals(this, baseTextEvent))
                return true;

            return base.Equals(baseTextEvent, respectDeltaTime) && Text == baseTextEvent.Text;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI meta event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        /// <exception cref="ArgumentOutOfRangeException">Text event cannot be read since the size is
        /// negative number.</exception>
        protected sealed override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Text event cannot be read since the size is negative number.");

            Text = reader.ReadString(size);
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected sealed override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteString(Text);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <returns>Size of the event's content.</returns>
        protected sealed override int GetContentSize()
        {
            return Text?.Length ?? 0;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
