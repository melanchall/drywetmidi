using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when the actual size of a MIDI file chunk differs from
    /// the one declared in its header.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.InvalidChunkSizePolicy"/>
    /// is set to <see cref="InvalidChunkSizePolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    public sealed class InvalidChunkSizeException : MidiException
    {
        #region Constructors

        internal InvalidChunkSizeException(string chunkId, long expectedSize, long actualSize)
            : base($"Actual size ({actualSize}) of a {chunkId} chunk differs from the one declared in the chunk's header ({expectedSize}).")
        {
            ChunkId = chunkId;
            ExpectedSize = expectedSize;
            ActualSize = actualSize;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ID of a chunk caused this exception.
        /// </summary>
        public string ChunkId { get; }

        /// <summary>
        /// Gets the expected size of a chunk written in its header.
        /// </summary>
        public long ExpectedSize { get; }

        /// <summary>
        /// Gets the actual size of a chunk.
        /// </summary>
        public long ActualSize { get; }

        #endregion
    }
}
