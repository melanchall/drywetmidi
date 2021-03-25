using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// A class representing the ID of a musical note.
    /// </summary>
    public sealed class NoteId
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteId"/> class.
        /// </summary>
        /// <param name="channel">The audio channel associated with the musical note.</param>
        /// <param name="noteNumber">The identification number associated with the musical note.</param>
        public NoteId(FourBitNumber channel, SevenBitNumber noteNumber)
        {
            Channel = channel;
            NoteNumber = noteNumber;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the audio channel associated with the musical note.
        /// </summary>
        public FourBitNumber Channel { get; }

        /// <summary>
        /// Gets the identification number associated with the musical note.
        /// </summary>
        public SevenBitNumber NoteNumber { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object against which to compare the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var noteId = obj as NoteId;
            if (ReferenceEquals(noteId, null))
                return false;

            return Channel == noteId.Channel &&
                   NoteNumber == noteId.NoteNumber;
        }

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Channel * 1000 + NoteNumber;
        }

        #endregion
    }
}
