using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Utilities related to IDs of musical notes.
    /// </summary>
    public static class NoteIdUtilities
    {
        #region Methods

        /// <summary>
        /// Gets the ID of the specified note.
        /// </summary>
        /// <param name="note">The note for which to get the ID.</param>
        /// <returns>The ID of the specific note.</returns>
        public static NoteId GetNoteId(this Note note)
        {
            return new NoteId(note.Channel, note.NoteNumber);
        }

        /// <summary>
        /// Gets the ID of the specified note event.
        /// </summary>
        /// <param name="noteEvent">The note event for which to get the ID.</param>
        /// <returns>The ID of the specified note event.</returns>
        public static NoteId GetNoteId(this NoteEvent noteEvent)
        {
            return new NoteId(noteEvent.Channel, noteEvent.NoteNumber);
        }

        #endregion
    }
}
