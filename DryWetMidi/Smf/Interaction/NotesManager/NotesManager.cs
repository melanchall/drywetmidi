using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class NotesManager : IDisposable
    {
        #region Fields

        private readonly TimedEventsManager _timedEventsManager;
        private readonly List<Note> _notes = new List<Note>();

        private bool _disposed;

        #endregion

        #region Constructor

        public NotesManager(EventsCollection eventsCollection, Comparison<MidiEvent> sameTimeEventsComparison = null)
        {
            if (eventsCollection == null)
                throw new ArgumentNullException(nameof(eventsCollection));

            _timedEventsManager = eventsCollection.ManageTimedEvents(sameTimeEventsComparison);
            _notes.AddRange(CreateNotes(_timedEventsManager.Events));
        }

        #endregion

        #region Properties

        public IEnumerable<Note> Notes => _notes.OrderBy(n => n.Time);

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

        public IEnumerable<Note> GetNotesAtTime(long time, bool exactMatch = true)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return _notes.Where(n => IsNoteAtTime(n, time, exactMatch));
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

        private static bool IsNoteAtTime(Note note, long time, bool exactMatch)
        {
            var noteTime = note.Time;
            if (noteTime == time)
                return true;

            if (!exactMatch)
                return false;

            return time > noteTime && time < noteTime + note.Length;
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
