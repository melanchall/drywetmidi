namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The type of a MIDI token.
    /// </summary>
    /// <seealso cref="MidiTokensReader"/>
    public enum MidiTokenType
    {
        /// <summary>
        /// A chunk's header. See <see cref="ChunkHeaderToken"/>.
        /// </summary>
        ChunkHeader,

        /// <summary>
        /// The data of a header chunk of a MIDI file. See <see cref="FileHeaderToken"/>.
        /// </summary>
        FileHeader,

        /// <summary>
        /// A MIDI event. See <see cref="MidiEventToken"/>.
        /// </summary>
        MidiEvent,

        /// <summary>
        /// A bytes packet. See <see cref="BytesPacketToken"/>.
        /// </summary>
        BytesPacket
    }
}
