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

        /// <summary>
        /// Initializes a new instance of the <see cref="NotesManager"/> with the specified events
        /// collection and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds note events to manage.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <remarks>
        /// If the <paramref name="sameTimeEventsComparison"/> is not specified events with the same time
        /// will be placed into the underlying events collection in order of adding them through the manager.
        /// If you want to specify custom order of such events you need to specify appropriate comparison delegate.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Gets the <see cref="TimedObjectsCollection{TObject}"/> with all notes managed by the current
        /// <see cref="NotesManager"/>.
        /// </summary>
        public TimedObjectsCollection<Note> Notes => Objects;

        #endregion
    }
}
