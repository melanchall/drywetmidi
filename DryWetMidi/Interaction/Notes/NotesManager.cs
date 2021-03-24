using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a way to manage notes of a MIDI file.
    /// </summary>
    /// <remarks>
    /// This manager is wrapper for the <see cref="TimedEventsManager"/> that provides easy manipulation
    /// of <see cref="NoteOnEvent"/> and <see cref="NoteOffEvent"/> events through the <see cref="Note"/>
    /// objects. To start manage notes you need to get an instance of the <see cref="NotesManager"/>. To
    /// finish managing you need to call the <see cref="SaveChanges"/> or <see cref="Dispose()"/> method.
    /// Since the manager implements <see cref="IDisposable"/> it is recommended to manage notes within
    /// using block.
    /// </remarks>
    public sealed class NotesManager : IDisposable
    {
        #region Fields

        private readonly TimedEventsManager _timedEventsManager;

        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NotesManager"/> with the specified events
        /// collection and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds note events to manage.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <remarks>
        /// If the <paramref name="sameTimeEventsComparison"/> is not specified events with the same time
        /// will be placed into the underlying events collection in order of adding them through the manager.
        /// If you want to specify custom order of such events you need to specify appropriate comparison delegate.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public NotesManager(EventsCollection eventsCollection, NoteDetectionSettings settings = null, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            _timedEventsManager = eventsCollection.ManageTimedEvents(sameTimeEventsComparison);

            Notes = new NotesCollection(_timedEventsManager.Events.GetNotesAndTimedEventsLazy(settings).OfType<Note>());
            Notes.CollectionChanged += OnNotesCollectionChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="NotesCollection"/> with all notes managed by the current <see cref="NotesManager"/>.
        /// </summary>
        public NotesCollection Notes { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Saves all notes that were managed with the current <see cref="NotesManager"/> updating
        /// underlying events collection.
        /// </summary>
        /// <remarks>
        /// This method will rewrite content of the events collection was used to construct the current
        /// <see cref="NotesManager"/> with events were managed by this manager. Also all delta-times
        /// of wrapped events will be recalculated according to the <see cref="Note.Time"/> and
        /// <see cref="Note.Length"/>.
        /// </remarks>
        public void SaveChanges()
        {
            foreach (var note in Notes)
            {
                var noteOnEvent = (NoteOnEvent)note.TimedNoteOnEvent.Event;
                var noteOffEvent = (NoteOffEvent)note.TimedNoteOffEvent.Event;

                noteOnEvent.Channel = noteOffEvent.Channel = note.Channel;
                noteOnEvent.NoteNumber = noteOffEvent.NoteNumber = note.NoteNumber;
                noteOnEvent.Velocity = note.Velocity;
                noteOffEvent.Velocity = note.OffVelocity;
            }

            _timedEventsManager.SaveChanges();
        }

        private void OnNotesCollectionChanged(NotesCollection collection, NotesCollectionChangedEventArgs args)
        {
            var addedNotes = args.AddedNotes;
            if (addedNotes != null)
                _timedEventsManager.Events.Add(GetNotesTimedEvents(addedNotes));

            var removedNotes = args.RemovedNotes;
            if (removedNotes != null)
                _timedEventsManager.Events.Remove(GetNotesTimedEvents(removedNotes));
        }

        private static IEnumerable<TimedEvent> GetNotesTimedEvents(IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            return notes.SelectMany(n => new[] { n.TimedNoteOnEvent, n.TimedNoteOffEvent });
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
                Notes.CollectionChanged -= OnNotesCollectionChanged;
                SaveChanges();
            }

            _disposed = true;
        }

        #endregion
    }
}
