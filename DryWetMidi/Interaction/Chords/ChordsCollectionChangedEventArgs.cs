using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides data for the <see cref="ChordsCollection.CollectionChanged"/> event.
    /// </summary>
    public sealed class ChordsCollectionChangedEventArgs : EventArgs
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChordsCollectionChangedEventArgs"/> class with the
        /// specified added chords and removed ones.
        /// </summary>
        /// <param name="addedChords">Chords that were added to a <see cref="ChordsCollection"/>.</param>
        /// <param name="removedChords">Chords that were removed from a <see cref="ChordsCollection"/>.</param>
        public ChordsCollectionChangedEventArgs(IEnumerable<Chord> addedChords, IEnumerable<Chord> removedChords)
        {
            AddedChords = addedChords;
            RemovedChords = removedChords;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets chords that were added to a <see cref="ChordsCollection"/>.
        /// </summary>
        public IEnumerable<Chord> AddedChords { get; }

        /// <summary>
        /// Gets chords that were removed from a <see cref="ChordsCollection"/>.
        /// </summary>
        public IEnumerable<Chord> RemovedChords { get; }

        #endregion
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="ChordsCollection.CollectionChanged"/> event raised when
    /// a <see cref="ChordsCollection"/> changed.
    /// </summary>
    /// <param name="collection"><see cref="ChordsCollection"/> that has fired the event.</param>
    /// <param name="args">A <see cref="ChordsCollectionChangedEventArgs"/> that contains the event data.</param>
    public delegate void ChordsCollectionChangedEventHandler(ChordsCollection collection, ChordsCollectionChangedEventArgs args);
}
