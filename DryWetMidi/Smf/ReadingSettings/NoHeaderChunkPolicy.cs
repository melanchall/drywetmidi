namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Specifies how reading engine should react on missing of the header chunk
    /// in the MIDI file. The default is <see cref="Abort"/>.
    /// </summary>
    public enum NoHeaderChunkPolicy
    {
        /// <summary>
        /// Abort reading and throw an <see cref="NoHeaderChunkException"/>.
        /// </summary>
        Abort = 0,

        /// <summary>
        /// Ignore missing of the header chunk.
        /// </summary>
        Ignore
    }
}
