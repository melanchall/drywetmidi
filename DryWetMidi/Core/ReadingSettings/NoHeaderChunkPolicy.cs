namespace Melanchall.DryWetMidi.Core
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
        /// Ignore missing of the header chunk. You'll be able specify time division manually
        /// after reading via <see cref="MidiFile.TimeDivision"/> property.
        /// </summary>
        Ignore
    }
}
