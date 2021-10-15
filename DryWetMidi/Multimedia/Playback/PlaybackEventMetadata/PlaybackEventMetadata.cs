namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class PlaybackEventMetadata
    {
        public NotePlaybackEventMetadata Note { get; set; }

        public TimedEventPlaybackEventMetadata TimedEvent { get; set; }
    }
}
