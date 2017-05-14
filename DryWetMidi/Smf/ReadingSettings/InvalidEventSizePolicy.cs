namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Specifies how reading engine should react on invalid expected size of an event.
    /// The default is <see cref="Ignore"/>.
    /// </summary>
    /// <remarks>
    /// Some events (at now, meta events only) are written along with the size of their content.
    /// For example, for the Set Tempo event 0 should be written as a size since this event has
    /// no parameters. If read size is not 0 and <see cref="Abort"/> option is used, an
    /// exception will be thrown.
    /// </remarks>
    public enum InvalidEventSizePolicy : byte
    {
        /// <summary>
        /// Ignore invalid declared size of an event.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Abort reading and throw an exception.
        /// </summary>
        Abort
    }
}
