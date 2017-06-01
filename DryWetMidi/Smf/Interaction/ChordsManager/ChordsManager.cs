using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class ChordsManager : IDisposable
    {
        #region Fields

        private readonly NotesManager _notesManager;
        private readonly List<Chord> _chords = new List<Chord>();
        private readonly long _chordNotesTolerance;

        private bool _disposed;

        #endregion

        #region Constructor

        public ChordsManager(EventsCollection eventsCollection, long chordNotesTolerance = 0, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            _chordNotesTolerance = chordNotesTolerance;
            _notesManager = eventsCollection.ManageNotes(sameTimeEventsComparison);
            _chords.AddRange(CreateChords(_notesManager.Notes, _chordNotesTolerance));
        }

        #endregion

        #region Properties

        public IEnumerable<Chord> Chords => _chords.OrderBy(c => c.Time);

        #endregion

        #region Methods

        public void SaveChanges()
        {
            _notesManager.SaveChanges();
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

                chord.AddNote(note);

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
                SaveChanges();

            _disposed = true;
        }

        #endregion
    }
}
