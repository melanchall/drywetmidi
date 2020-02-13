using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Core
{
    public sealed class MidiChunkEqualityComparer : IEqualityComparer<MidiChunk>
    {
        #region Fields

        private readonly MidiChunkEqualityCheckSettings _settings;

        #endregion

        #region Constructor

        public MidiChunkEqualityComparer()
            : this(null)
        {
        }

        public MidiChunkEqualityComparer(MidiChunkEqualityCheckSettings settings)
        {
            _settings = settings ?? new MidiChunkEqualityCheckSettings();
        }

        #endregion

        #region IEqualityComparer<MidiChunk>

        public bool Equals(MidiChunk x, MidiChunk y)
        {
            string message;
            return MidiChunk.Equals(x, y, _settings, out message);
        }

        public int GetHashCode(MidiChunk obj)
        {
            return obj?.ChunkId.GetHashCode() ?? 0;
        }

        #endregion
    }
}
