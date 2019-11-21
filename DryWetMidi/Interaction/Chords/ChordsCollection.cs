using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
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

        /// <summary>
        /// Performs an action when objects are added to the collection.
        /// </summary>
        /// <param name="addedObjects">Collection of added objects.</param>
        protected override void OnObjectsAdded(IEnumerable<Chord> addedObjects)
        {
            base.OnObjectsAdded(addedObjects);

            OnCollectionChanged(addedObjects, null);
        }

        /// <summary>
        /// Performs an action when objects are removed from the collection.
        /// </summary>
        /// <param name="removedObjects">Collection of removed objects.</param>
        protected override void OnObjectsRemoved(IEnumerable<Chord> removedObjects)
        {
            base.OnObjectsRemoved(removedObjects);

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
