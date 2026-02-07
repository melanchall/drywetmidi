using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class NoteIdUtilities
    {
        #region Methods

        public static int GetNoteId(this NoteEvent noteEvent)
        {
            return (noteEvent.Channel << 7) | noteEvent.NoteNumber;
        }

        public static int GetNoteId(this Note note)
        {
            return (note.Channel << 7) | note.NoteNumber;
        }

        #endregion
    }
}
