namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Specifies how reading engine should react on missed End Of Track event.
    /// The default is <see cref="Ignore"/>.
    /// </summary>
    /// <remarks>
    /// Although the End Of Track event is not optional and therefore missing of it
    /// must be treated as error, you can try to read a track chunk relying on the chunk's size only.
    /// </remarks>
    public enum MissedEndOfTrackPolicy : byte
    {
        /// <summary>
        /// Ignore missing of the End Of Track event and try to read a track chunk relying on
        /// the chunk's size.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Abort reading and throw an <see cref="MissedEndOfTrackEventException"/>.
        /// </summary>
        Abort
    }
}
