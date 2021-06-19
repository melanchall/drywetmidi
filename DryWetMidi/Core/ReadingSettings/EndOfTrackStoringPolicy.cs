namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on End Of Track event encountered.
    /// The default is <see cref="Omit"/>.
    /// </summary>
    public enum EndOfTrackStoringPolicy
    {
        /// <summary>
        /// Omit an event and don't store it to the <see cref="TrackChunk.Events"/>.
        /// </summary>
        Omit = 0,

        /// <summary>
        /// Store an event to the <see cref="TrackChunk.Events"/>.
        /// </summary>
        Store
    }
}
