using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class PlaybackNotesDictionaryEntry
    {
        #region Constructor

        public PlaybackNotesDictionaryEntry(NoteId noteId, Note note)
        {
            NoteId = noteId;
            Note = note;
        }

        #endregion

        #region Properties

        public NoteId NoteId { get; }

        public Note Note { get; }

        #endregion
    }
}
