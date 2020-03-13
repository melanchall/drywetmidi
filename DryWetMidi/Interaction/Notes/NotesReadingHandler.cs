using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using static Melanchall.DryWetMidi.Interaction.GetTimedEventsAndNotesUtilities;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Collects notes during MIDI data reading.
    /// </summary>
    /// <remarks>
    /// This handler can be added to the <see cref="ReadingSettings.ReadingHandlers"/> collection to
    /// collect notes along with MIDI data reading. Scope of the handler is <c>TargetScope.Event | TargetScope.File</c>.
    /// </remarks>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="NotesReadingHandler"/> with the specified
        /// value indicating whether notes should be automatically sorted on <see cref="Notes"/> get.
        /// </summary>
        /// <param name="sortNotes">A value indicating whether notes should be automatically sorted
        /// on <see cref="Notes"/> get.</param>
        public NotesReadingHandler(bool sortNotes)
            : base(TargetScope.Event | TargetScope.File)
        {
            _sortNotes = sortNotes;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the notes collected during MIDI data reading. If the current <see cref="NotesReadingHandler"/>
        /// was created with <c>sortNotes</c> set to <c>true</c>, the returned notes will be sorted by start time.
        /// </summary>
        public IEnumerable<Note> Notes => _notesProcessed ??
            (_notesProcessed = (_sortNotes ? _notes.OrderBy(e => e.Time) : (IEnumerable<Note>)_notes).ToList());

        #endregion

        #region Overrides

        /// <summary>
        /// Initializes handler. This method will be called before reading MIDI data.
        /// </summary>
        public override void Initialize()
        {
            _noteEventsDescriptors.Clear();
            _eventsTail.Object = null;

            _notes.Clear();
            _notesProcessed = null;
        }

        /// <summary>
        /// Handles finish of file reading. Called after file is read.
        /// </summary>
        /// <param name="midiFile">MIDI file read.</param>
        public override void OnFinishFileReading(MidiFile midiFile)
        {
            foreach (var timedObject in _noteEventsDescriptors.SelectMany(d => d.GetTimedObjects()))
            {
                AddNote(timedObject);
            }
        }

        /// <summary>
        /// Handles finish of MIDI event reading. Called after MIDI event is read and before
        /// putting it to <see cref="TrackChunk.Events"/> collection.
        /// </summary>
        /// <param name="midiEvent">MIDI event read.</param>
        /// <param name="absoluteTime">Absolute time of <paramref name="midiEvent"/>.</param>
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
