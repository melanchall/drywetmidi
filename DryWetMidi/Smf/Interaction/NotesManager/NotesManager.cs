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

        public void AddNotes(IEnumerable<Note> notes)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            _timedEventsManager.AddEvents(GetNotesTimedEvents(notes));
            _notes.AddRange(notes);
        }

        public void AddNotes(params Note[] notes)
        {
            AddNotes((IEnumerable<Note>)notes);
        }

        public void RemoveNotes(IEnumerable<Note> notes)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            _timedEventsManager.RemoveEvents(GetNotesTimedEvents(notes));

            foreach (var n in notes.ToList())
            {
                _notes.Remove(n);
            }
        }

        public void RemoveNotes(params Note[] notes)
        {
            RemoveNotes((IEnumerable<Note>)notes);
        }

        public void RemoveAllNotes()
        {
            _timedEventsManager.RemoveEvents(GetNotesTimedEvents(_notes));
            _notes.Clear();
        }

        public void RemoveAllNotes(Predicate<Note> predicate)
        {
            _timedEventsManager.RemoveEvents(GetNotesTimedEvents(_notes.Where(n => predicate(n))));
            _notes.RemoveAll(predicate);
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
                SaveChanges();

            _disposed = true;
        }

        #endregion
    }
}
