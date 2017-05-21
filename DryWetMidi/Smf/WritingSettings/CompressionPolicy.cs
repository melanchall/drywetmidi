using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Specifies how writing engine should compress MIDI data. The default is <see cref="NoCompression"/>.
    /// </summary>
    [Flags]
    public enum CompressionPolicy
    {
        /// <summary>
        /// Don't use any compression on the MIDI data to write.
        /// </summary>
        NoCompression = 0,

        /// <summary>
        /// Use default compression on the MIDI data to write. This option turns on all options
        /// that don't lead to data losing (for example, unknown meta events).
        /// </summary>
        Default =
            UseRunningStatus |
            NoteOffAsSilentNoteOn |
            DeleteDefaultTimeSignature |
            DeleteDefaultKeySignature |
            DeleteDefaultSetTempo,

        /// <summary>
        /// Use 'running status' to turn off writing of the status bytes of consecutive events
        /// of the same type.
        /// </summary>
        UseRunningStatus = 1,

        /// <summary>
        /// Turn Note Off events into the Note On ones with zero velocity. Note that it helps to
        /// compress MIDI data in the case of <see cref="UseRunningStatus"/> is used only.
        /// </summary>
        NoteOffAsSilentNoteOn = 2,

        /// <summary>
        /// Don't write default Time Signature event.
        /// </summary>
        DeleteDefaultTimeSignature = 4,

        /// <summary>
        /// Don't write default Key Signature event.
        /// </summary>
        DeleteDefaultKeySignature = 8,

        /// <summary>
        /// Don't write default Set Tempo event.
        /// </summary>
        DeleteDefaultSetTempo = 16,

        /// <summary>
        /// Don't write instances of the <see cref="UnknownMetaEvent"/>.
        /// </summary>
        DeleteUnknownMetaEvents = 32,

        /// <summary>
        /// Don't write instances of the <see cref="UnknownChunk"/>.
        /// </summary>
        DeleteUnknownChunks = 64,
    }
}
