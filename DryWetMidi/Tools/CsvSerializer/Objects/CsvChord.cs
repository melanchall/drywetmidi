using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class CsvChord : CsvObject
    {
        #region Constructor

        public CsvChord(int? chunkIndex, string chunkId, int? objectIndex)
            : base(chunkIndex, chunkId, objectIndex)
        {
        }

        #endregion

        #region Properties

        public List<CsvNote> Notes {  get; } = new List<CsvNote>();

        #endregion
    }
}
