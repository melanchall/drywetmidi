namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on new track chunk if already read chunks
    /// count greater or equals the one declared at the file header. The default is <see cref="Read"/>.
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
