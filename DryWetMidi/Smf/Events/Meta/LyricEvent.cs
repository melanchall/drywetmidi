using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Lyric meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI lyrics meta message shows the lyrics of a song at a particular time in the MIDI sequence.
    /// </remarks>
    public sealed class LyricEvent : BaseTextEvent, IEquatable<LyricEvent>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LyricEvent"/>.
        /// </summary>
        public LyricEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LyricEvent"/> with the
        /// specified text of lyrics.
        /// </summary>
        /// <param name="text">Text of the lyrics.</param>
        public LyricEvent(string text)
            : base(text)
        {
        }

        #endregion

        #region IEquatable<LyricEvent>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="lyricEvent">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(LyricEvent lyricEvent)
        {
            return Equals(lyricEvent, true);
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
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="midiEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the delta-times will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public override bool Equals(MidiEvent midiEvent, bool respectDeltaTime)
        {
            return Equals(midiEvent as LyricEvent, respectDeltaTime);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Lyric ({Text})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as LyricEvent);
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
