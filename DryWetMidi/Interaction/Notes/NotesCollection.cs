using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Collection of <see cref="Note"/> objects.
    /// </summary>
    public sealed class NotesCollection : TimedObjectsCollection<Note>
    {
        #region Events

        /// <summary>
        /// Occurs when notes collection changes.
        /// </summary>
        public event NotesCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Constructor

        internal NotesCollection(IEnumerable<Note> notes)
            : base(notes)
        {
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Performs an action when objects are added to the collection.
        /// </summary>
        /// <param name="addedObjects">Collection of added objects.</param>
        protected override void OnObjectsAdded(IEnumerable<Note> addedObjects)
        {
            base.OnObjectsAdded(addedObjects);

            OnCollectionChanged(addedObjects, null);
        }

        /// <summary>
        /// Performs an action when objects are removed from the collection.
        /// </summary>
        /// <param name="removedObjects">Collection of removed objects.</param>
        protected override void OnObjectsRemoved(IEnumerable<Note> removedObjects)
        {
            base.OnObjectsRemoved(removedObjects);

            OnCollectionChanged(null, removedObjects);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Fires the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="addedNotes">Notes added to the <see cref="NotesCollection"/>.</param>
        /// <param name="removedNotes">Notes removed from the <see cref="NotesCollection"/>.</param>
        private void OnCollectionChanged(IEnumerable<Note> addedNotes, IEnumerable<Note> removedNotes)
        {
            CollectionChanged?.Invoke(this, new NotesCollectionChangedEventArgs(addedNotes, removedNotes));
        }

        #endregion
    }
}
