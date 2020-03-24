using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Core
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ticksPerQuarterNote"/> is negative.</exception>
        public TicksPerQuarterNoteTimeDivision(short ticksPerQuarterNote)
        {
            ThrowIfArgument.IsNegative(nameof(ticksPerQuarterNote),
                                        ticksPerQuarterNote,
                                        "Ticks per quarter-note must be non-negative number.");

            TicksPerQuarterNote = ticksPerQuarterNote;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets number of ticks which make up a quarter-note.
        /// </summary>
        public short TicksPerQuarterNote { get; } = DefaultTicksPerQuarterNote;

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="TicksPerQuarterNoteTimeDivision"/> objects are equal.
        /// </summary>
        /// <param name="timeDivision1">The first <see cref="TicksPerQuarterNoteTimeDivision"/> to compare.</param>
        /// <param name="timeDivision2">The second <see cref="TicksPerQuarterNoteTimeDivision"/> to compare.</param>
        /// <returns><c>true</c> if the time divisions are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(TicksPerQuarterNoteTimeDivision timeDivision1, TicksPerQuarterNoteTimeDivision timeDivision2)
        {
            if (ReferenceEquals(timeDivision1, timeDivision2))
                return true;

            if (ReferenceEquals(null, timeDivision1) || ReferenceEquals(null, timeDivision2))
                return false;

            return timeDivision1.TicksPerQuarterNote == timeDivision2.TicksPerQuarterNote;
        }

        /// <summary>
        /// Determines if two <see cref="TicksPerQuarterNoteTimeDivision"/> objects are not equal.
        /// </summary>
        /// <param name="timeDivision1">The first <see cref="TicksPerQuarterNoteTimeDivision"/> to compare.</param>
        /// <param name="timeDivision2">The second <see cref="TicksPerQuarterNoteTimeDivision"/> to compare.</param>
        /// <returns><c>false</c> if the time divisions are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(TicksPerQuarterNoteTimeDivision timeDivision1, TicksPerQuarterNoteTimeDivision timeDivision2)
        {
            return !(timeDivision1 == timeDivision2);
        }

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

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{TicksPerQuarterNote} ticks/qnote";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as TicksPerQuarterNoteTimeDivision);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return TicksPerQuarterNote.GetHashCode();
        }

        #endregion
    }
}
