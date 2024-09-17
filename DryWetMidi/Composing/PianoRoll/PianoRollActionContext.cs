using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
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

        public MusicTheory.Note Note { get; }

        public int CellsNumber { get; }

        public ITimeSpan Length { get; }

        #endregion
    }
}
