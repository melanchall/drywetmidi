namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on missed <c>End Of Track</c> event.
    /// The default is <see cref="Ignore"/>.
    /// </summary>
    /// <remarks>
    /// <para>Although the <c>End Of Track</c> event is not optional and therefore missing of it
    /// should be treated as error, you can try to read a track chunk relying on the chunk's size only.</para>
    /// </remarks>
    public enum MissedEndOfTrackPolicy : byte
    {
        /// <summary>
        /// Ignore missing of the <c>End Of Track</c> event and try to read a track chunk relying on
        /// the chunk's size.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Abort reading and throw an <see cref="MissedEndOfTrackEventException"/>.
        /// </summary>
        Abort
    }
}
