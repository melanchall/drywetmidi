using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a way to manage chords of a MIDI file.
    /// </summary>
    /// <remarks>
    /// This manager is wrapper for the <see cref="NotesManager"/> that provides easy manipulation
    /// of sets of <see cref="NoteOnEvent"/> and <see cref="NoteOffEvent"/> events through the <see cref="Chord"/>
    /// objects. To start manage chords you need to get an instance of the <see cref="ChordsManager"/>. To
    /// finish managing you need to call the <see cref="SaveChanges"/> or <see cref="Dispose()"/> method.
    /// Since the manager implements <see cref="IDisposable"/> it is recommended to manage chords within
    /// using block.
    /// </remarks>
    public sealed class ChordsManager : IDisposable
    {
        #region Fields

        private readonly NotesManager _notesManager;

        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChordsManager"/> with the specified events
        /// collection, notes tolerance and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds chord events to manage.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public ChordsManager(EventsCollection eventsCollection, ChordDetectionSettings settings = null, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            _notesManager = eventsCollection.ManageNotes(settings?.NoteDetectionSettings, sameTimeEventsComparison);

            Chords = new ChordsCollection(_notesManager.Notes.GetChords(settings));
            Chords.CollectionChanged += OnChordsCollectionChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="ChordsCollection"/> with all chords managed by the current <see cref="ChordsManager"/>.
        /// </summary>
        public ChordsCollection Chords { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Saves all chords that were managed with the current <see cref="ChordsManager"/> updating
        /// underlying events collection.
        /// </summary>
        /// <remarks>
        /// This method will rewrite content of the events collection was used to construct the current
        /// <see cref="ChordsManager"/> with events were managed by this manager. Also all delta-times
        /// of wrapped events will be recalculated according to the <see cref="Note.Time"/> and
        /// <see cref="Note.Length"/> of chords notes.
        /// </remarks>
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
            ThrowIfArgument.IsNull(nameof(chord), chord);

            chord.NotesCollectionChanged += OnChordNotesCollectionChanged;
        }

        private void UnsubscribeFromChordEvents(Chord chord)
        {
            ThrowIfArgument.IsNull(nameof(chord), chord);

            chord.NotesCollectionChanged -= OnChordNotesCollectionChanged;
        }

        private void AddNotes(IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            _notesManager.Notes.Add(notes);
        }

        private void RemoveNotes(IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            _notesManager.Notes.Remove(notes);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
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
