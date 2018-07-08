using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Note Off message.
    /// </summary>
    /// <remarks>
    /// This message is sent when a note is released (ended).
    /// </remarks>
    public sealed class NoteOffEvent : NoteEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOffEvent"/>.
        /// </summary>
        public NoteOffEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteOffEvent"/> with the specified
        /// note number and velocity.
        /// </summary>
        /// <param name="noteNumber">Note number.</param>
        /// <param name="velocity">Velocity.</param>
        public NoteOffEvent(SevenBitNumber noteNumber, SevenBitNumber velocity)
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
            return $"Note Off [{Channel}] ({NoteNumber}, {Velocity})";
        }

        #endregion
    }
}
