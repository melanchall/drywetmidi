using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class NotesCollection : IEnumerable<Note>
    {
        #region Events

        public event NotesCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Fields

        private readonly List<Note> _notes = new List<Note>();

        #endregion

        #region Constructor

        public NotesCollection()
        {
        }

        public NotesCollection(IEnumerable<Note> notes)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            _notes.AddRange(notes);
        }

        #endregion

        #region Methods

        public void Add(IEnumerable<Note> notes)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            _notes.AddRange(notes);
            OnNotesAdded(notes);
        }

        public void Add(params Note[] notes)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            Add((IEnumerable<Note>)notes);
        }

        public void Remove(IEnumerable<Note> notes)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            foreach (var n in notes.ToList())
            {
                _notes.Remove(n);
            }

            OnNotesRemoved(notes);
        }

        public void Remove(params Note[] notes)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            Remove((IEnumerable<Note>)notes);
        }

        public void RemoveAll(Predicate<Note> predicate)
        {
            var removedNotes = _notes.Where(n => predicate(n)).ToList();
            _notes.RemoveAll(predicate);
            OnNotesRemoved(removedNotes);
        }

        public void Clear()
        {
            var removedNotes = _notes.ToList();
            _notes.Clear();
            OnNotesRemoved(removedNotes);
        }

        public IEnumerable<Note> GetAtTime(long time, bool exactMatch = true)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return _notes.Where(n => IsNoteAtTime(n, time, exactMatch));
        }

        private static bool IsNoteAtTime(Note note, long time, bool exactMatch)
        {
            var noteTime = note.Time;
            if (noteTime == time)
                return true;

            if (!exactMatch)
                return false;

            return time > noteTime && time < noteTime + note.Length;
        }

        private void OnNotesAdded(IEnumerable<Note> addedNotes)
        {
            OnCollectionChanged(addedNotes, null);
        }

        private void OnNotesRemoved(IEnumerable<Note> removedNotes)
        {
            OnCollectionChanged(null, removedNotes);
        }

        private void OnCollectionChanged(IEnumerable<Note> addedNotes, IEnumerable<Note> removedNotes)
        {
            CollectionChanged?.Invoke(this, new NotesCollectionChangedEventArgs(addedNotes, removedNotes));
        }

        #endregion

        #region IEnumerable<Note>

        public IEnumerator<Note> GetEnumerator()
        {
            return _notes.OrderBy(n => n.Time).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
