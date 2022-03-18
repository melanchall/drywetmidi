using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods for splitting chords.
    /// </summary>
    /// <remarks>
    /// See <see href="xref:a_notes_chords_splitter">Notes/chords splitter</see> article to learn more.
    /// </remarks>
    public sealed class ChordsSplitter : LengthedObjectsSplitter<Chord>
    {
        #region Overrides

        /// <summary>
        /// Splits an object by the specified time.
        /// </summary>
        /// <param name="obj">Object to split.</param>
        /// <param name="time">Time to split <paramref name="obj"/> by.</param>
        /// <returns>An object containing left and right parts of the split object.</returns>
        protected override SplitLengthedObject<Chord> SplitObject(Chord obj, long time)
        {
            return obj.Split(time);
        }

        #endregion
    }
}
