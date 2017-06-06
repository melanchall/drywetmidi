using System;

namespace Melanchall.DryWetMidi.Smf
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
        /// Initializes a new instance of the <see cref="TicksPerQuarterNoteTimeDivision"/>.
        /// </summary>
        public TicksPerQuarterNoteTimeDivision()
        {
        }

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
        /// Gets or sets number of ticks which make up a quarter-note.
        /// </summary>
        public short TicksPerQuarterNote { get; set; } = DefaultTicksPerQuarterNote;

        #endregion

        #region Overrides

        internal override short ToInt16()
        {
            return TicksPerQuarterNote;
        }

        /// <summary>
        /// Clones time division by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the time division.</returns>
        public override TimeDivision Clone()
        {
            return new TicksPerQuarterNoteTimeDivision(TicksPerQuarterNote);
        }

        #endregion
    }
}
