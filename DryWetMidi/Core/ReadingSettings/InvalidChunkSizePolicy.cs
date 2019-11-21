namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on difference between actual
    /// chunk's size and the one declared in its header. The default is <see cref="Abort"/>.
    /// </summary>
    public enum InvalidChunkSizePolicy : byte
    {
        /// <summary>
        /// Abort reading and throw an <see cref="InvalidChunkSizeException"/>.
        /// </summary>
        Abort = 0,

        /// <summary>
        /// Ignore difference between actual chunk's size and the declared one.
        /// </summary>
        Ignore
    }
}
