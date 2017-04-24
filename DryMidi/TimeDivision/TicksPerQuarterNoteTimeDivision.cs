using System;

namespace Melanchall.DryMidi
{
    /// <summary>
    /// Time division that represents number of delta-time "ticks" which make up a quarter-note.
    /// </summary>
    public sealed class TicksPerQuarterNoteTimeDivision : TimeDivision
    {
        #region Constants

        /// <summary>
        /// Default number of ticks which make up a quarter-note.
        /// </summary>
        public const short DefaultTicksPerQuarterNote = 96;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TicksPerQuarterNoteTimeDivision"/> with
        /// the specified ticks number per a quarter-note.
        /// </summary>
        /// <param name="ticksPerQuarterNote">Number of ticks which make up a quarter-note.</param>
        public TicksPerQuarterNoteTimeDivision(short ticksPerQuarterNote)
        {
            if (ticksPerQuarterNote < 0)
                throw new ArgumentException("Ticks per quarter-note must be non-negative number.", nameof(ticksPerQuarterNote));

            TicksPerQuarterNote = ticksPerQuarterNote;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of ticks which make up a quarter-note.
        /// </summary>
        public short TicksPerQuarterNote { get; }

        #endregion

        #region Overrides

        internal override short ToInt16()
        {
            return TicksPerQuarterNote;
        }

        #endregion
    }
}
