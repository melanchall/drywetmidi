namespace Melanchall.DryWetMidi.Core
{
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

        public string ChunkId { get; }

        public uint ChunkContentSize { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Chunk header token (chunk ID = {ChunkId}, content size = {ChunkContentSize})";
        }

        #endregion
    }
}
