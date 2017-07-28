using Melanchall.DryWetMidi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Collection of <see cref="Chord"/> objects.
    /// </summary>
    public sealed class ChordsCollection : IEnumerable<Chord>
    {
        #region Events

        /// <summary>
        /// Occurs when chords collection changes.
        /// </summary>
        public event ChordsCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Fields

        private readonly List<Chord> _chords = new List<Chord>();

        #endregion

        #region Constructor
        
        internal ChordsCollection(IEnumerable<Chord> chords)
        {
            ThrowIfArgument.IsNull(nameof(chords), chords);

            _chords.AddRange(chords.Where(c => c != null));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds chords to the <see cref="ChordsCollection"/>.
        /// </summary>
        /// <param name="chords">Chords to add to the <see cref="ChordsCollection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="chords"/> is null.</exception>
        public void Add(IEnumerable<Chord> chords)
        {
            ThrowIfArgument.IsNull(nameof(chords), chords);

            var addedChords = chords.Where(c => c != null).ToList();
            _chords.AddRange(addedChords);
            OnChordsAdded(addedChords);
        }

        /// <summary>
        /// Adds chords to the <see cref="ChordsCollection"/>.
        /// </summary>
        /// <param name="chords">Chords to add to the <see cref="ChordsCollection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="chords"/> is null.</exception>
        public void Add(params Chord[] chords)
        {
            ThrowIfArgument.IsNull(nameof(chords), chords);

            Add((IEnumerable<Chord>)chords);
        }

        /// <summary>
        /// Removes chords from the <see cref="ChordsCollection"/>.
        /// </summary>
        /// <param name="chords">Chords to remove from the <see cref="ChordsCollection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="chords"/> is null.</exception>
        public void Remove(IEnumerable<Chord> chords)
        {
            ThrowIfArgument.IsNull(nameof(chords), chords);

            var removedChords = new List<Chord>();
            foreach (var c in chords)
            {
                if (_chords.Remove(c))
                    removedChords.Add(c);
            }

            OnChordsRemoved(removedChords);
        }

        /// <summary>
        /// Removes chords from the <see cref="ChordsCollection"/>.
        /// </summary>
        /// <param name="chords">Chords to remove from the <see cref="ChordsCollection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="chords"/> is null.</exception>
        public void Remove(params Chord[] chords)
        {
            ThrowIfArgument.IsNull(nameof(chords), chords);

            Remove((IEnumerable<Chord>)chords);
        }

        /// <summary>
        /// Removes all the chords that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions of
        /// the chords to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> is null.</exception>
        public void RemoveAll(Predicate<Chord> match)
        {
            ThrowIfArgument.IsNull(nameof(match), match);

            var removedChords = _chords.Where(c => match(c)).ToList();
            _chords.RemoveAll(match);
            OnChordsRemoved(removedChords);
        }

        /// <summary>
        /// Removes all chords from the <see cref="ChordsCollection"/>.
        /// </summary>
        public void Clear()
        {
            var removedChords = _chords.ToList();
            _chords.Clear();
            OnChordsRemoved(removedChords);
        }

        /// <summary>
        /// Fires the <see cref="CollectionChanged"/> event when chords added to the <see cref="ChordsCollection"/>.
        /// </summary>
        /// <param name="addedChords">Added chords.</param>
        private void OnChordsAdded(IEnumerable<Chord> addedChords)
        {
            OnCollectionChanged(addedChords, null);
        }

        /// <summary>
        /// Fires the <see cref="CollectionChanged"/> event when chords removed from the <see cref="ChordsCollection"/>.
        /// </summary>
        /// <param name="removedChords">Removed chords.</param>
        private void OnChordsRemoved(IEnumerable<Chord> removedChords)
        {
            OnCollectionChanged(null, removedChords);
        }

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

        #region IEnumerable<Chord>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Chord> GetEnumerator()
        {
            return _chords.OrderBy(n => n.Time).GetEnumerator();
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
