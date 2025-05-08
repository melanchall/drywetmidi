using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Holds notes collection for <see cref="Playback.NotesPlaybackStarted"/> and
    /// <see cref="Playback.NotesPlaybackFinished"/>.
    /// </summary>
    public sealed class NotesEventArgs : EventArgs
    {
        #region Constructor

        internal NotesEventArgs(
            Note[] notes,
            Note[] originalNotes)
        {
            Notes = notes;
            OriginalNotes = originalNotes;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the actual notes that started or finished to play by <see cref="Playback"/>.
        /// They can differ from the original ones, for example, due to <see cref="Playback.NoteCallback"/>
        /// is used. To get original notes use the <see cref="OriginalNotes"/> property.
        /// </summary>
        /// <remarks>
        /// Count of notes within this collection will be equal to the count of original notes
        /// (see <see cref="OriginalNotes"/>).
        /// </remarks>
        public ICollection<Note> Notes { get; }

        /// <summary>
        /// Gets the original notes that started or finished to play by <see cref="Playback"/>.
        /// </summary>
        /// <remarks>
        /// Count of notes within this collection will be equal to the count of processed notes
        /// (see <see cref="Notes"/>).
        /// </remarks>
        public ICollection<Note> OriginalNotes { get; }

        #endregion
    }
}
