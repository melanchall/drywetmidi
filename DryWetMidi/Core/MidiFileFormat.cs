namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Format of a Standard MIDI file which specifies overall structure of the file.
    /// </summary>
    public enum MidiFileFormat : ushort
    {
        /// <summary>
        /// The file contains a single multi-channel track.
        /// </summary>
        /// <remarks>
        /// File of this format has a header chunk followed by one track chunk. It is the most
        /// interchangeable representation of data.
        /// </remarks>
        SingleTrack = 0,

        /// <summary>
        /// The file contains one or more simultaneous tracks (or MIDI outputs) of a sequence.
        /// </summary>
        /// <remarks>
        /// File of this format has a header chunk followed by one or more track chunks. First track
        /// chunk is usually reserved for tempo information (it can contain events like
        /// <see cref="SetTempoEvent"/>, <see cref="TimeSignatureEvent"/>, <see cref="KeySignatureEvent"/>).
        /// </remarks>
        MultiTrack = 1,

        /// <summary>
        /// The file contains one or more sequentially independent single-track patterns.
        /// </summary>
        MultiSequence = 2
    }
}
