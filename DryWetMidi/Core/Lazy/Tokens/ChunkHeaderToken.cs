namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a chunk's header.
    /// </summary>
    /// <seealso cref="MidiTokensReader"/>
    public sealed class ChunkHeaderToken : MidiToken
    {
        #region Constructor

        internal ChunkHeaderToken(string chunkId, uint chunkContentSize)
            : base(MidiTokenType.ChunkHeader)
        {
            ChunkId = chunkId;
            ChunkContentSize = chunkContentSize;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ID of a chunk.
        /// </summary>
        public string ChunkId { get; }

        /// <summary>
        /// Gets the size (in bytes) of a chunk's content.
        /// </summary>
        public uint ChunkContentSize { get; }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Chunk header token (chunk ID = {ChunkId}, content size = {ChunkContentSize})";
        }

        #endregion
    }
}
