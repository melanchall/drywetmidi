using System;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how writing engine should compress MIDI data. The default is <see cref="NoCompression"/>.
    /// </summary>
    [Obsolete("OBS1")]
    [Flags]
    public enum CompressionPolicy
    {
        /// <summary>
        /// Don't use any compression on the MIDI data to write. All data will be written as is.
        /// </summary>
        NoCompression = 0,

        /// <summary>
        /// Use default compression on the MIDI data to write. This option turns on all options
        /// that don't lead to data losing (for example, unknown meta events).
        /// </summary>
        /// <remarks>
        /// <para>The option combines following rules:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><see cref="UseRunningStatus"/></description>
        /// </item>
        /// <item>
        /// <description><see cref="NoteOffAsSilentNoteOn"/></description>
        /// </item>
        /// <item>
        /// <description><see cref="DeleteDefaultTimeSignature"/></description>
        /// </item>
        /// <item>
        /// <description><see cref="DeleteDefaultKeySignature"/></description>
        /// </item>
        /// <item>
        /// <description><see cref="DeleteDefaultSetTempo"/></description>
        /// </item>
        /// </list>
        /// </remarks>
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
        /// Turn Note Off events into the <c>Note On</c> ones with zero velocity. Note that it helps to
        /// compress MIDI data only if <see cref="UseRunningStatus"/> is used.
        /// </summary>
        NoteOffAsSilentNoteOn = 2,

        /// <summary>
        /// Don't write default <c>Time Signature</c> events if there are no non-default ones before them.
        /// </summary>
        DeleteDefaultTimeSignature = 4,

        /// <summary>
        /// Don't write default <c>Key Signature</c> events if there are no non-default ones before them.
        /// </summary>
        DeleteDefaultKeySignature = 8,

        /// <summary>
        /// Don't write default <c>Set Tempo</c> events if there are no non-default ones before them.
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
