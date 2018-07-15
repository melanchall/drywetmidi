namespace Melanchall.DryWetMidi.Tools
{
    internal enum RecordType
    {
        Header,
        TrackChunkStart,
        TrackChunkEnd,
        FileEnd,
        Event,
        Note
    }
}
