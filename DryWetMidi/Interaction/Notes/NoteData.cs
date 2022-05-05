using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Holds the data for a <see cref="Note"/> construction.
    /// </summary>
    /// <seealso cref="NoteDetectionSettings"/>
    /// <seealso cref="NotesManagingUtilities"/>
    public sealed class NoteData
    {
        #region Constructor

        internal NoteData(TimedEvent timedNoteOnEvent, TimedEvent timedNoteOffEvent)
        {
            TimedNoteOnEvent = timedNoteOnEvent;
            TimedNoteOffEvent = timedNoteOffEvent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets <see cref="TimedEvent"/> holding <see cref="NoteOnEvent"/> of the note.
        /// </summary>
        public TimedEvent TimedNoteOnEvent { get; }

        /// <summary>
        /// Gets <see cref="TimedEvent"/> holding <see cref="NoteOffEvent"/> of the note.
        /// </summary>
        public TimedEvent TimedNoteOffEvent { get; }

        #endregion
    }
}
