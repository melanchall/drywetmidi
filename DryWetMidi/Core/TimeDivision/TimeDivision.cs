namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// MIDI file time division.
    /// </summary>
    /// <remarks>
    /// Time division specifies the meaning of the delta-times of events. There are two types of
    /// the time division: ticks per quarter note and SMPTE. Time division of the first type has bit 15 set
    /// to 0. In this case bits 14 thru 0 represent the number of ticks which make up a quarter-note.
    /// Division of the second type has bit 15 set to 1. In this case bits 14 thru 8 contain one of the four
    /// values: -24, -25, -29, or -30, corresponding to the four standard SMPTE and MIDI Time Code formats
    /// (-29 corresponds to 30 drop frame), and represents the number of frames per second. Bits 7 thru 0
    /// (which represent a byte stored positive) is the resolution within a frame: typical values may be 4
    /// (MIDI Time Code resolution), 8, 10, 80 (bit resolution), or 100.
    /// </remarks>
    public abstract class TimeDivision
    {
        #region Methods

        /// <summary>
        /// Converts this time division into 16-bit signed integer number that will be written to a file.
        /// </summary>
        /// <returns>16-bit signed integer number representing this time division.</returns>
        internal abstract short ToInt16();

        /// <summary>
        /// Clones time division by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the time division.</returns>
        public abstract TimeDivision Clone();

        #endregion
    }
}
