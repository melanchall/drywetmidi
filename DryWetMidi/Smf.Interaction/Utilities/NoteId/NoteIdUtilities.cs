namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class NoteIdUtilities
    {
        #region Methods

        public static NoteId GetNoteId(this Note note)
        {
            return new NoteId(note.Channel, note.NoteNumber);
        }

        public static NoteId GetNoteId(this NoteEvent noteEvent)
        {
            return new NoteId(noteEvent.Channel, noteEvent.NoteNumber);
        }

        #endregion
    }
}
