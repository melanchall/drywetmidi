using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class ChordsCollection : IEnumerable<Chord>
    {
        #region Events

        public event ChordsCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Fields

        private readonly List<Chord> _chords = new List<Chord>();

        #endregion

        #region Constructor
        
        internal ChordsCollection(IEnumerable<Chord> chords)
        {
            if (chords == null)
                throw new ArgumentNullException(nameof(chords));

            _chords.AddRange(chords);
        }

        #endregion

        #region Methods

        public void Add(IEnumerable<Chord> chords)
        {
            if (chords == null)
                throw new ArgumentNullException(nameof(chords));

            _chords.AddRange(chords);
            OnChordsAdded(chords);
        }

        public void Add(params Chord[] chords)
        {
            if (chords == null)
                throw new ArgumentNullException(nameof(chords));

            Add((IEnumerable<Chord>)chords);
        }

        public void Remove(IEnumerable<Chord> chords)
        {
            if (chords == null)
                throw new ArgumentNullException(nameof(chords));

            var removedChords = chords.ToList();

            foreach (var c in removedChords)
            {
                _chords.Remove(c);
            }

            OnChordsRemoved(removedChords);
        }

        public void Remove(params Chord[] chords)
        {
            if (chords == null)
                throw new ArgumentNullException(nameof(chords));

            Remove((IEnumerable<Chord>)chords);
        }

        public void RemoveAll(Predicate<Chord> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            var removedChords = _chords.Where(c => match(c)).ToList();
            _chords.RemoveAll(match);
            OnChordsRemoved(removedChords);
        }

        public void Clear()
        {
            var removedChords = _chords.ToList();
            _chords.Clear();
            OnChordsRemoved(removedChords);
        }

        private void OnChordsAdded(IEnumerable<Chord> addedChords)
        {
            OnCollectionChanged(addedChords, null);
        }

        private void OnChordsRemoved(IEnumerable<Chord> removedChords)
        {
            OnCollectionChanged(null, removedChords);
        }

        private void OnCollectionChanged(IEnumerable<Chord> addedChords, IEnumerable<Chord> removedChords)
        {
            CollectionChanged?.Invoke(this, new ChordsCollectionChangedEventArgs(addedChords, removedChords));
        }

        #endregion

        #region IEnumerable<Chord>

        public IEnumerator<Chord> GetEnumerator()
        {
            return _chords.OrderBy(n => n.Time).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
