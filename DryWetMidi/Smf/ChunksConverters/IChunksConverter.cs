using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Provides a way to convert a collection of <see cref="MidiChunk"/> objects to
    /// another representation according to rules defined by implementation of this
    /// interface.
    /// </summary>
    internal interface IChunksConverter
    {
        /// <summary>
        /// Converts collection of <see cref="MidiChunk"/> objects to another representation.
        /// </summary>
        /// <param name="chunks">Chunks collection that need to be converted.</param>
        /// <param name="cloneEvents">If true events in track chunks will be cloned for the new chunks;
        /// if false events will be just moved to the new track chunks.</param>
        /// <returns>Another representation of the <paramref name="chunks"/> collection.</returns>
        IEnumerable<MidiChunk> Convert(IEnumerable<MidiChunk> chunks, bool cloneEvents = true);
    }
}
