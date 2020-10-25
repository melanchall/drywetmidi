namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should buffer incoming MIDI data before reading it.
    /// The default is <see cref="UseFixedSizeBuffer"/>.
    /// </summary>
    public enum BufferingPolicy
    {
        /// <summary>
        /// Use buffer of fixed size specified by <see cref="ReaderSettings.BufferSize"/>.
        /// </summary>
        UseFixedSizeBuffer = 0,

        /// <summary>
        /// Don't buffer data and read it from stream as is.
        /// </summary>
        DontUseBuffering,

        /// <summary>
        /// Use buffer specified by <see cref="ReaderSettings.Buffer"/>.
        /// </summary>
        UseCustomBuffer,

        /// <summary>
        /// Put entire MIDI data to buffer in memory and read it from here.
        /// </summary>
        BufferAllData
    }
}
