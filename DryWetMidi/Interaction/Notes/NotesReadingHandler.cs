using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using static Melanchall.DryWetMidi.Interaction.GetTimedEventsAndNotesUtilities;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class NotesReadingHandler : ReadingHandler
    {
        #region Fields

        private readonly bool _sortNotes;
        private readonly List<Note> _notes = new List<Note>();

        private IEnumerable<Note> _notesProcessed;

        private readonly List<NoteEventsDescriptor> _noteEventsDescriptors = new List<NoteEventsDescriptor>();
        private readonly ObjectWrapper<List<TimedEvent>> _eventsTail = new ObjectWrapper<List<TimedEvent>>();

        #endregion

        #region Constructor

        public NotesReadingHandler(bool sortNotes)
            : base(TargetScope.Event | TargetScope.File)
        {
            _sortNotes = sortNotes;
        }

        #endregion

        #region Properties

        public IEnumerable<Note> Notes => _notesProcessed ??
            (_notesProcessed = (_sortNotes ? _notes.OrderBy(e => e.Time) : (IEnumerable<Note>)_notes).ToList());

        #endregion

        #region Overrides

        public override void Initialize()
        {
            _noteEventsDescriptors.Clear();
            _eventsTail.Object = null;

            _notes.Clear();
            _notesProcessed = null;
        }

        public override void OnFinishFileReading(MidiFile midiFile)
        {
            foreach (var timedObject in _noteEventsDescriptors.SelectMany(d => d.GetTimedObjects()))
            {
                AddNote(timedObject);
            }
        }

        public override void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
        {
            if (midiEvent.EventType != MidiEventType.NoteOn && midiEvent.EventType != MidiEventType.NoteOff)
                return;

            foreach (var timedObject in GetTimedEventsAndNotes(new TimedEvent(midiEvent, absoluteTime), _noteEventsDescriptors, _eventsTail))
            {
                AddNote(timedObject);
            }
        }

        #endregion

        #region Methods

        private void AddNote(ITimedObject timedObject)
        {
            var note = timedObject as Note;
            if (note == null)
                return;

            _notes.Add(note);
        }

        #endregion
    }
}
