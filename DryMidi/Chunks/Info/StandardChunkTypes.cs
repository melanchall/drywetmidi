namespace Melanchall.DryMidi
{
    internal static class StandardChunkTypes
    {
        internal static readonly ChunkTypesCollection Types = new ChunkTypesCollection
        {
            { typeof(HeaderChunk), ChunkIds.Header },
            { typeof(TrackChunk), ChunkIds.Track }
        };
    }
}
