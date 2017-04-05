using System;

namespace Melanchall.DryMidi
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
        /// that don't lead to data losing (for example, unknown meta messages).
        /// </summary>
        Default =
            UseRunningStatus |
            NoteOffAsSilentNoteOn |
            DeleteDefaultTimeSignature |
            DeleteDefaultKeySignature |
            DeleteDefaultSetTempo |
            DeleteRedundantMessages,

        /// <summary>
        /// Use 'running status' to turn off writing of the status bytes of consecutive messages
        /// of the same type.
        /// </summary>
        UseRunningStatus = 1,

        /// <summary>
        /// Turn Note Off messages into the Note On ones with zero velocity. Note that it helps to
        /// compress MIDI data in the case of <see cref="UseRunningStatus"/> is used only.
        /// </summary>
        NoteOffAsSilentNoteOn = 2,

        /// <summary>
        /// Don't write default Time Signature message.
        /// </summary>
        DeleteDefaultTimeSignature = 4,

        /// <summary>
        /// Don't write default key signature message.
        /// </summary>
        DeleteDefaultKeySignature = 8,

        /// <summary>
        /// Don't write default Set Tempo message.
        /// </summary>
        DeleteDefaultSetTempo = 16,

        /// <summary>
        /// Don't write unknown meta messages presented in an instance of the <see cref="MidiFile"/>.
        /// </summary>
        DeleteUnknownMetaMessages = 32,

        /// <summary>
        /// Don't write unknown chunks presented in an instance of the <see cref="MidiFile"/>.
        /// </summary>
        DeleteUnknownChunks = 64,

        /// <summary>
        /// Don't write redundant messages.
        /// </summary>
        DeleteRedundantMessages = 128
    }
}
