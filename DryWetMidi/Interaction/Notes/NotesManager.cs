using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a way to manage notes of a MIDI file.
    /// </summary>
    [Obsolete("OBS11")]
    public sealed class NotesManager : TimedObjectsManager<Note>
    {
        #region Constructor

        public NotesManager(
            EventsCollection eventsCollection,
            NoteDetectionSettings noteDetectionSettings = null,
            Comparison<MidiEvent> sameTimeEventsComparison = null)
            : base(
                  eventsCollection,
                  ObjectType.Note,
                  new ObjectDetectionSettings
                  {
                      NoteDetectionSettings = noteDetectionSettings
                  },
                  new TimedObjectsComparerOnSameEventTime(sameTimeEventsComparison))
        {
        }

        #endregion

        #region Properties

        public TimedObjectsCollection<Note> Notes => Objects;

        #endregion
    }
}
