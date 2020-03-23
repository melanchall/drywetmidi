using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Utilities related to the IDs of musical notes.
    /// </summary>
    public static class NoteIdUtilities
    {
        #region Methods

        /// <summary>
        /// Gets the ID of the specified musical note.
        /// </summary>
        /// <param name="note">The musical note for which to get the ID.</param>
        /// <returns>The ID of the specified musical note.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="note"/> is <c>null</c>.</exception>
        public static NoteId GetNoteId(this Note note)
        {
            ThrowIfArgument.IsNull(nameof(note), note);

            return new NoteId(note.Channel, note.NoteNumber);
        }

        /// <summary>
        /// Gets the ID of the specified musical note event.
        /// </summary>
        /// <param name="noteEvent">The musical note event for which to get the ID.</param>
        /// <returns>The ID of the specified musical note event.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="noteEvent"/> is <c>null</c>.</exception>
        public static NoteId GetNoteId(this NoteEvent noteEvent)
        {
            ThrowIfArgument.IsNull(nameof(noteEvent), noteEvent);

            return new NoteId(noteEvent.Channel, noteEvent.NoteNumber);
        }

        #endregion
    }
}
