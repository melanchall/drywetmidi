using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides data for the <see cref="NotesCollection.CollectionChanged"/> event.
    /// </summary>
    public sealed class NotesCollectionChangedEventArgs : EventArgs
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NotesCollectionChangedEventArgs"/> class with the
        /// specified added notes and removed ones.
        /// </summary>
        /// <param name="addedNotes">Notes that were added to a <see cref="NotesCollection"/>.</param>
        /// <param name="removedNotes">Notes that were removed from a <see cref="NotesCollection"/>.</param>
        public NotesCollectionChangedEventArgs(IEnumerable<Note> addedNotes, IEnumerable<Note> removedNotes)
        {
            AddedNotes = addedNotes;
            RemovedNotes = removedNotes;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets notes that were added to a <see cref="NotesCollection"/>.
        /// </summary>
        public IEnumerable<Note> AddedNotes { get; }

        /// <summary>
        /// Gets notes that were removed from a <see cref="NotesCollection"/>.
        /// </summary>
        public IEnumerable<Note> RemovedNotes { get; }

        #endregion
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="NotesCollection.CollectionChanged"/> event raised when
    /// a <see cref="NotesCollection"/> changed.
    /// </summary>
    /// <param name="collection"><see cref="NotesCollection"/> that has fired the event.</param>
    /// <param name="args">A <see cref="NotesCollectionChangedEventArgs"/> that contains the event data.</param>
    public delegate void NotesCollectionChangedEventHandler(NotesCollection collection, NotesCollectionChangedEventArgs args);
}
