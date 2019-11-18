namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// A class encapsulating metadata related to a playback event.
    /// </summary>
    public sealed class PlaybackEventMetadata
    {
        /// <summary>
        /// Gets the musical note associated with the playback event.
        /// </summary>
        public NotePlaybackEventMetadata Note { get; set; }
    }
}
