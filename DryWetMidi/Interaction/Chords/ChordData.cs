using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Holds the data for a <see cref="Chord"/> construction.
    /// </summary>
    /// <seealso cref="ChordDetectionSettings"/>
    /// <seealso cref="ChordsManagingUtilities"/>
    public sealed class ChordData
    {
        #region Constructor

        internal ChordData(ICollection<Note> notes)
        {
            Notes = notes;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets notes collection to build a chord.
        /// </summary>
        public ICollection<Note> Notes { get; }

        #endregion
    }
}
