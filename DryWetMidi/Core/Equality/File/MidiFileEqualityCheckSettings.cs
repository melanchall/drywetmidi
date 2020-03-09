namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Holds settings according to which <see cref="MidiFile"/> objects should
    /// be compared for equality.
    /// </summary>
    public sealed class MidiFileEqualityCheckSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="MidiFile.OriginalFormat"/> values
        /// should be compared or not.
        /// </summary>
        /// <remarks>
        /// In <see cref="MidiFile"/> objects created by reading MIDI data from file or stream
        /// <see cref="MidiFile.OriginalFormat"/> is always set if format was valid. But if format was invalid or
        /// <see cref="MidiFile"/> is created from scratch using constrictor, the property will throw
        /// exception so you may want to exclude this property from comparison process.
        /// </remarks>
        public bool CompareOriginalFormat { get; set; } = true;

        /// <summary>
        /// Gets or sets settings according to which <see cref="MidiChunk"/> objects should
        /// be compared for equality.
        /// </summary>
        public MidiChunkEqualityCheckSettings ChunkEqualityCheckSettings { get; set; } = new MidiChunkEqualityCheckSettings();

        #endregion
    }
}
