namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how the reading engine should react to an unexpected number of track chunks. The default is
    /// <see cref="Ignore"/>.
    /// </summary>
    public enum UnexpectedTrackChunksCountPolicy
    {
        /// <summary>
        /// Ignore an unexpected number of track chunks.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Abort reading and throw an <see cref="UnexpectedTrackChunksCountException"/>.
        /// </summary>
        Abort
    }
}
