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
        /// Gets notes collection that started or finished to play by a <see cref="Playback"/>.
        /// </summary>
        public ICollection<Note> Notes { get; }

        public ICollection<Note> OriginalNotes { get; }

        #endregion
    }
}
