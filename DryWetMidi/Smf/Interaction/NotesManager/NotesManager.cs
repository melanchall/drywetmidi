using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class NotesManager : IDisposable
    {
        #region Fields

        private readonly TimedEventsManager _timedEventsManager;

        private bool _disposed;

        #endregion

        #region Constructor

        public NotesManager(EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            _timedEventsManager = eventsCollection.ManageTimedEvents(sameTimeEventsComparison);

            Notes = new NotesCollection(CreateNotes(_timedEventsManager.Events));
            Notes.CollectionChanged += OnNotesCollectionChanged;
        }

        #endregion

        #region Properties

        public NotesCollection Notes { get; }

        #endregion

        #region Methods

        public void SaveChanges()
        {
            foreach (var note in Notes)
            {
                var noteOnEvent = (NoteOnEvent)note.TimedNoteOnEvent.Event;
                var noteOffEvent = (NoteOffEvent)note.TimedNoteOffEvent.Event;

                noteOnEvent.Channel = noteOffEvent.Channel = note.Channel;
                noteOnEvent.NoteNumber = noteOffEvent.NoteNumber = note.NoteNumber;
                noteOnEvent.Velocity = note.Velocity;
                noteOffEvent.Velocity = (SevenBitNumber)0;
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

        private static IEnumerable<Note> CreateNotes(IEnumerable<TimedEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            var noteOnTimedEvents = new List<TimedEvent>();

            foreach (var timedEvent in events)
            {
                var midiEvent = timedEvent.Event;
                if (midiEvent is NoteOnEvent)
                {
                    noteOnTimedEvents.Add(timedEvent);
                    continue;
                }

                var noteOffEvent = midiEvent as NoteOffEvent;
                if (noteOffEvent != null)
                {
                    var channel = noteOffEvent.Channel;
                    var noteNumber = noteOffEvent.NoteNumber;
                    var noteOnTimedEvent = noteOnTimedEvents.FirstOrDefault(e => IsAppropriateNoteOnTimedEvent(e, channel, noteNumber));

                    if (noteOnTimedEvent == null)
                        continue;

                    noteOnTimedEvents.Remove(noteOnTimedEvent);
                    yield return new Note(noteOnTimedEvent, timedEvent);
                }
            }
        }

        private static bool IsAppropriateNoteOnTimedEvent(TimedEvent timedEvent, FourBitNumber channel, SevenBitNumber noteNumber)
        {
            var noteOnEvent = timedEvent.Event as NoteOnEvent;
            return noteOnEvent != null &&
                   noteOnEvent.Channel == channel &&
                   noteOnEvent.NoteNumber == noteNumber;
        }

        private static IEnumerable<TimedEvent> GetNotesTimedEvents(IEnumerable<Note> notes)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            return notes.SelectMany(n => new[] { n.TimedNoteOnEvent, n.TimedNoteOffEvent });
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
                Notes.CollectionChanged -= OnNotesCollectionChanged;
                SaveChanges();
            }

            _disposed = true;
        }

        #endregion
    }
}
