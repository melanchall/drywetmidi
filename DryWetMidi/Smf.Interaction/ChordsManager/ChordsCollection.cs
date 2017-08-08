using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Collection of <see cref="Chord"/> objects.
    /// </summary>
    public sealed class ChordsCollection : TimedObjectsCollection<Chord>
    {
        #region Events

        /// <summary>
        /// Occurs when chords collection changes.
        /// </summary>
        public event ChordsCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Constructor
        
        internal ChordsCollection(IEnumerable<Chord> chords)
            : base(chords)
        {
        }

        #endregion

        #region Overrides

        protected override void OnObjectsAdded(IEnumerable<Chord> addedObjects)
        {
            OnCollectionChanged(addedObjects, null);
        }

        protected override void OnObjectsRemoved(IEnumerable<Chord> removedObjects)
        {
            OnCollectionChanged(null, removedObjects);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Fires the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="addedChords">Chords added to the <see cref="ChordsCollection"/>.</param>
        /// <param name="removedChords">Chords removed from the <see cref="ChordsCollection"/>.</param>
        private void OnCollectionChanged(IEnumerable<Chord> addedChords, IEnumerable<Chord> removedChords)
        {
            CollectionChanged?.Invoke(this, new ChordsCollectionChangedEventArgs(addedChords, removedChords));
        }

        #endregion
    }
}
