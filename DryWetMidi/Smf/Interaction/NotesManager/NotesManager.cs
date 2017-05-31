using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class NotesManager : IDisposable
    {
        #region Fields

        private bool _disposed;

        private readonly TimedEventsManager _timedEventsManager;
        private readonly List<Note> _notes;

        #endregion

        #region Constructor

        public NotesManager(EventsCollection eventsCollection)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            _timedEventsManager = new TimedEventsManager(eventsCollection);
            _notes = CreateNotes(_timedEventsManager.Events).ToList();
        }

        #endregion

        #region Properties

        public IEnumerable<Note> Notes => _notes.OrderBy(e => e.Time);

        #endregion

        #region Methods

        public void AddNote(Note note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            _timedEventsManager.AddEvent(note.TimedNoteOnEvent);
            _timedEventsManager.AddEvent(note.TimedNoteOffEvent);
            _notes.Add(note);
        }

        public void RemoveNote(Note note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            _timedEventsManager.RemoveEvent(note.TimedNoteOnEvent);
            _timedEventsManager.RemoveEvent(note.TimedNoteOffEvent);
            _notes.Remove(note);
        }

        public void SaveChanges()
        {
            foreach (var note in _notes)
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

                    noteOnTimedEvents.RemoveAll(e => IsAppropriateNoteOnTimedEvent(e, channel, noteNumber));
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
