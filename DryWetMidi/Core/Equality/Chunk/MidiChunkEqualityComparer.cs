using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Core
{
    // TODO: test
    /// <summary>
    /// Comparer to compare <see cref="MidiChunk"/> objects for equality.
    /// </summary>
    public sealed class MidiChunkEqualityComparer : IEqualityComparer<MidiChunk>
    {
        #region Fields

        private readonly MidiChunkEqualityCheckSettings _settings;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiChunkEqualityComparer"/>.
        /// </summary>
        public MidiChunkEqualityComparer()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiChunkEqualityComparer"/> with the
        /// specified settings according to which <see cref="MidiChunk"/> objects should
        /// be compared for equality.
        /// </summary>
        /// <param name="settings">Settings according to which <see cref="MidiChunk"/> objects should
        /// be compared for equality.</param>
        public MidiChunkEqualityComparer(MidiChunkEqualityCheckSettings settings)
        {
            _settings = settings ?? new MidiChunkEqualityCheckSettings();
        }

        #endregion

        #region IEqualityComparer<MidiChunk>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first chunk to compare.</param>
        /// <param name="y">The second chunk to compare.</param>
        /// <returns><c>true</c> if the specified objects are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(MidiChunk x, MidiChunk y)
        {
            string message;
            return MidiChunk.Equals(x, y, _settings, out message);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="MidiChunk"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(MidiChunk obj)
        {
            return obj.ChunkId.GetHashCode();
        }

        #endregion
    }
}
