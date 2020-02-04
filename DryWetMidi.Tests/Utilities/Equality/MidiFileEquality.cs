using System.Linq;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class MidiFileEquality
    {
        #region Methods

        public static bool AreEqual(MidiFile file1, MidiFile file2, bool compareOriginalFormat)
        {
            if (ReferenceEquals(file1, file2))
                return true;

            if (ReferenceEquals(null, file1) || ReferenceEquals(null, file2))
                return false;

            if (compareOriginalFormat && file1.OriginalFormat != file2.OriginalFormat)
                return false;

            if (!ReferenceEquals(file1.TimeDivision, file2.TimeDivision) && !file1.TimeDivision.Equals(file2.TimeDivision))
                return false;

            var chunks1 = file1.Chunks;
            var chunks2 = file2.Chunks;

            if (chunks1.Count != chunks2.Count)
                return false;

            return chunks1.Zip(chunks2, (c1, c2) => new { Chunk1 = c1, Chunk2 = c2 })
                          .All(c => MidiChunkEquality.AreEqual(c.Chunk1, c.Chunk2));
        }

        #endregion
    }
}
