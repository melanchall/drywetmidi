namespace Melanchall.DryMidi
{
    /// <summary>
    /// Specifies how reading engine should react on invalid expected size of a message.
    /// The default is <see cref="Ignore"/>.
    /// </summary>
    /// <remarks>
    /// Some messages (at now, meta messages only) are written along with the size of their content.
    /// For example, for the Set Tempo message 0 should be written as a size since this message has
    /// no parameters. If read size is not 0 and <see cref="Abort"/> option is used, an
    /// exception will be thrown.
    /// </remarks>
    public enum InvalidMessageSizePolicy : byte
    {
        /// <summary>
        /// Ignore invalid declared size of a message.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Abort reading and throw an exception.
        /// </summary>
        Abort
    }
}
