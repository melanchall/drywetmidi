namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Specifies how reading engine should react on lack of bytes in the underlying stream
    /// that are needed to read some value (for example, DWORD requires 4 bytes available).
    /// The default is <see cref="Abort"/>.
    /// </summary>
    public enum NotEnoughBytesPolicy
    {
        /// <summary>
        /// Abort reading and throw an <see cref="NotEnoughBytesException"/>.
        /// </summary>
        Abort = 0,

        /// <summary>
        /// Ignore lack of bytes in the reader's underlying stream and just end reading.
        /// </summary>
        Ignore
    }
}
