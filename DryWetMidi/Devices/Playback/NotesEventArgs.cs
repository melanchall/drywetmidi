using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Holds notes collection for <see cref="Playback.NotesPlaybackStarted"/> and
    /// <see cref="Playback.NotesPlaybackFinished"/>.
    /// </summary>
    public sealed class NotesEventArgs : EventArgs
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NotesEventArgs"/> class.
        /// </summary>
        /// <param name="notes">The collection of notes that started or finished playing using a <see cref="Playback"/> object.</param>
        public NotesEventArgs(params Note[] notes)
        {
            Notes = notes;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets notes collection that started or finished to play by a <see cref="Playback"/>.
        /// </summary>
        public IEnumerable<Note> Notes { get; }

        #endregion
    }
}
