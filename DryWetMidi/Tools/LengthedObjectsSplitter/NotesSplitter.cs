using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods for splitting notes.
    /// </summary>
    /// <remarks>
    /// See <see href="xref:a_notes_chords_splitter">Notes/chords splitter</see> article to learn more.
    /// </remarks>
    public sealed class NotesSplitter : LengthedObjectsSplitter<Note>
    {
        #region Overrides

        /// <summary>
        /// Clones an object by creating a copy of it.
        /// </summary>
        /// <param name="obj">Object to clone.</param>
        /// <returns>Copy of the <paramref name="obj"/>.</returns>
        protected override Note CloneObject(Note obj)
        {
            return obj.Clone();
        }

        /// <summary>
        /// Splits an object by the specified time.
        /// </summary>
        /// <param name="obj">Object to split.</param>
        /// <param name="time">Time to split <paramref name="obj"/> by.</param>
        /// <returns>An object containing left and right parts of the split object.</returns>
        protected override SplitLengthedObject<Note> SplitObject(Note obj, long time)
        {
            return obj.Split(time);
        }

        #endregion
    }
}
