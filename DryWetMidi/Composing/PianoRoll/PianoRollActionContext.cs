using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Provides context for a custom piano roll action. More info in the
    /// <see href="xref:a_composing_pattern#customization">Pattern: Piano roll: Customization</see> article.
    /// </summary>
    public sealed class PianoRollActionContext
    {
        #region Constructor

        internal PianoRollActionContext(
            MusicTheory.Note note,
            int cellsNumber,
            ITimeSpan length)
        {
            Note = note;
            CellsNumber = cellsNumber;
            Length = length;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the note is being processed by the piano roll engine.
        /// </summary>
        public MusicTheory.Note Note { get; }

        /// <summary>
        /// Gets the number of cells an action should take into account. For
        /// a single-cell action this value is always <c>1</c>.
        /// </summary>
        public int CellsNumber { get; }

        /// <summary>
        /// Gets the length (duration) of a cells span. The number of cells within the span is <see cref="CellsNumber"/>.
        /// The type of returned value is defined by the type of the <see cref="PatternBuilder.NoteLength"/>
        /// property value.
        /// </summary>
        public ITimeSpan Length { get; }

        #endregion
    }
}
