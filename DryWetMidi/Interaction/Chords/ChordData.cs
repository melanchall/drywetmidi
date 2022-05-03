using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ChordData
    {
        #region Constructor

        internal ChordData(ICollection<Note> notes)
        {
            Notes = notes;
        }

        #endregion

        #region Properties

        public ICollection<Note> Notes { get; }

        #endregion
    }
}
