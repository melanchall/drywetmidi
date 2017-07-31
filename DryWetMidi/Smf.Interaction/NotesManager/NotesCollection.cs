﻿using Melanchall.DryWetMidi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Collection of <see cref="Note"/> objects.
    /// </summary>
    public sealed class NotesCollection : IEnumerable<Note>
    {
        #region Events

        /// <summary>
        /// Occurs when notes collection changes.
        /// </summary>
        public event NotesCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Fields

        private readonly List<Note> _notes = new List<Note>();

        #endregion

        #region Constructor

        internal NotesCollection(IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            _notes.AddRange(notes.Where(n => n != null));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds notes to the <see cref="NotesCollection"/>.
        /// </summary>
        /// <param name="notes">Notes to add to the <see cref="NotesCollection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null.</exception>
        public void Add(IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            var addedNotes = notes.Where(n => n != null).ToList();
            _notes.AddRange(addedNotes);
            OnNotesAdded(addedNotes);
        }

        /// <summary>
        /// Adds notes to the <see cref="NotesCollection"/>.
        /// </summary>
        /// <param name="notes">Notes to add to the <see cref="NotesCollection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null.</exception>
        public void Add(params Note[] notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            Add((IEnumerable<Note>)notes);
        }

        /// <summary>
        /// Removes notes from the <see cref="NotesCollection"/>.
        /// </summary>
        /// <param name="notes">Notes to remove from the <see cref="NotesCollection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null.</exception>
        public void Remove(IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            var removedNotes = new List<Note>();
            foreach (var n in notes.ToList())
            {
                if (_notes.Remove(n))
                    removedNotes.Add(n);
            }

            OnNotesRemoved(removedNotes);
        }

        /// <summary>
        /// Removes notes from the <see cref="NotesCollection"/>.
        /// </summary>
        /// <param name="notes">Notes to remove from the <see cref="NotesCollection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null.</exception>
        public void Remove(params Note[] notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            Remove((IEnumerable<Note>)notes);
        }

        /// <summary>
        /// Removes all the notes that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions of
        /// the notes to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> is null.</exception>
        public void RemoveAll(Predicate<Note> match)
        {
            ThrowIfArgument.IsNull(nameof(match), match);

            var removedNotes = _notes.Where(n => match(n)).ToList();
            _notes.RemoveAll(match);
            OnNotesRemoved(removedNotes);
        }

        /// <summary>
        /// Removes all notes from the <see cref="NotesCollection"/>.
        /// </summary>
        public void Clear()
        {
            var removedNotes = _notes.ToList();
            _notes.Clear();
            OnNotesRemoved(removedNotes);
        }

        /// <summary>
        /// Fires the <see cref="CollectionChanged"/> event when notes added to the <see cref="NotesCollection"/>.
        /// </summary>
        /// <param name="addedNotes">Added notes.</param>
        private void OnNotesAdded(IEnumerable<Note> addedNotes)
        {
            OnCollectionChanged(addedNotes, null);
        }

        /// <summary>
        /// Fires the <see cref="CollectionChanged"/> event when notes removed from the <see cref="NotesCollection"/>.
        /// </summary>
        /// <param name="removedNotes">Removed notes.</param>
        private void OnNotesRemoved(IEnumerable<Note> removedNotes)
        {
            OnCollectionChanged(null, removedNotes);
        }

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

        #region IEnumerable<Note>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Note> GetEnumerator()
        {
            return _notes.OrderBy(n => n.Time).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}