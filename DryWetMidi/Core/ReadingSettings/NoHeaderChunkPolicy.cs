namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on missing of the header chunk
    /// in the MIDI file. The default is <see cref="Abort"/>.
    /// </summary>
    public enum NoHeaderChunkPolicy
    {
        /// <summary>
        /// Abort reading and throw a <see cref="NoHeaderChunkException"/>.
        /// </summary>
        Abort = 0,

        /// <summary>
        /// Ignore the missing header chunk. You'll be able to specify the time division manually
        /// after reading via <see cref="MidiFile.TimeDivision"/> property.
        /// </summary>
        Ignore
    }
}
