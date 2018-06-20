using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Note On message.
    /// </summary>
    /// <remarks>
    /// This message is sent when a note is depressed (start).
    /// </remarks>
    public sealed class NoteOnEvent : NoteEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOnEvent"/>.
        /// </summary>
        public NoteOnEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOnEvent"/> with the specified
        /// note number and velocity.
        /// </summary>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="velocity">Velocity.</param>
        public NoteOnEvent(SevenBitNumber noteNumber, SevenBitNumber velocity)
            : base(noteNumber, velocity)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Note On [{Channel}] ({NoteNumber}, {Velocity})";
        }

        #endregion
    }
}
