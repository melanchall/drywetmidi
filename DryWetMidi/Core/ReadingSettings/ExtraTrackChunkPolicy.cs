namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on new track chunk if already read chunks
    /// count is greater than or equal to the one declared in the file header. The default is <see cref="Read"/>.
    /// </summary>
    public enum ExtraTrackChunkPolicy : byte
    {
        /// <summary>
        /// Read a track chunk anyway.
        /// </summary>
        Read = 0,

        /// <summary>
        /// Skip chunk and go to the next one.
        /// </summary>
        Skip
    }
}
