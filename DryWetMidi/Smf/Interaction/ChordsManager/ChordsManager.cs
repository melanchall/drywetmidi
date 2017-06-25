using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class ChordsManager : IDisposable
    {
        #region Fields

        private readonly NotesManager _notesManager;

        private bool _disposed;

        #endregion

        #region Constructor

        public ChordsManager(EventsCollection eventsCollection, long chordNotesTolerance = 0, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            _notesManager = eventsCollection.ManageNotes(sameTimeEventsComparison);

            Chords = new ChordsCollection(CreateChords(_notesManager.Notes, chordNotesTolerance));
            Chords.CollectionChanged += OnChordsCollectionChanged;
        }

        #endregion

        #region Properties

        public ChordsCollection Chords { get; }

        #endregion

        #region Methods

        public void SaveChanges()
        {
            _notesManager.SaveChanges();
        }

        private void OnChordsCollectionChanged(ChordsCollection collection, ChordsCollectionChangedEventArgs args)
        {
            var addedChords = args.AddedChords;
            if (addedChords != null)
            {
                foreach (var chord in addedChords)
                {
                    AddNotes(chord.Notes);
                    SubscribeToChordEvents(chord);
                }
            }

            var removedChords = args.RemovedChords;
            if (removedChords != null)
            {
                foreach (var chord in removedChords)
                {
                    RemoveNotes(chord.Notes);
                    UnsubscribeFromChordEvents(chord);
                }
            }
        }

        private void OnChordNotesCollectionChanged(NotesCollection collection, NotesCollectionChangedEventArgs args)
        {
            var addedNotes = args.AddedNotes;
            if (addedNotes != null)
                AddNotes(addedNotes);

            var removedNotes = args.RemovedNotes;
            if (removedNotes != null)
                RemoveNotes(removedNotes);
        }

        private void SubscribeToChordEvents(Chord chord)
        {
            if (chord == null)
                throw new ArgumentNullException(nameof(chord));

            chord.NotesCollectionChanged += OnChordNotesCollectionChanged;
        }

        private void UnsubscribeFromChordEvents(Chord chord)
        {
            if (chord == null)
                throw new ArgumentNullException(nameof(chord));

            chord.NotesCollectionChanged -= OnChordNotesCollectionChanged;
        }

        private void AddNotes(IEnumerable<Note> notes)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            _notesManager.Notes.Add(notes);
        }

        private void RemoveNotes(IEnumerable<Note> notes)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            _notesManager.Notes.Remove(notes);
        }

        private static IEnumerable<Chord> CreateChords(IEnumerable<Note> notes, long chordNotesTolerance)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            var lastNoteEndTime = long.MinValue;
            Chord chord = null;

            foreach (var note in notes)
            {
                var noteTime = note.Time;
                if (noteTime >= lastNoteEndTime || noteTime - chord.Time > chordNotesTolerance)
                {
                    if (chord != null)
                        yield return chord;

                    chord = new Chord();
                }

                chord.Notes.Add(note);

                lastNoteEndTime = noteTime + note.Length;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                foreach (var chord in Chords)
                {
                    UnsubscribeFromChordEvents(chord);
                }

                Chords.CollectionChanged -= OnChordsCollectionChanged;

                SaveChanges();
            }

            _disposed = true;
        }

        #endregion
    }
}
