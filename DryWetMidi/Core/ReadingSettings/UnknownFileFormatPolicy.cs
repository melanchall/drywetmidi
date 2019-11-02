namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on file format which doesn't belong to
    /// the <see cref="MidiFileFormat"/>.
    /// </summary>
    public enum UnknownFileFormatPolicy
    {
        /// <summary>
        /// Ignore unknown file format and try to read chunks.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Abort reading and throw an <see cref="UnknownFileFormatException"/>.
        /// </summary>
        Abort
    }
}
