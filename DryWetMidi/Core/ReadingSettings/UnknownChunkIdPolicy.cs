namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on chunk with unknown ID. The default
    /// is <see cref="ReadAsUnknownChunk"/>.
    /// </summary>
    public enum UnknownChunkIdPolicy : byte
    {
        /// <summary>
        /// Read the chunk as <see cref="UnknownChunk"/>.
        /// </summary>
        ReadAsUnknownChunk = 0,

        /// <summary>
        /// Skip this chunk and go to the next one.
        /// </summary>
        Skip,

        /// <summary>
        /// Abort reading and throw an <see cref="UnknownChunkException"/>.
        /// </summary>
        Abort
    }
}
