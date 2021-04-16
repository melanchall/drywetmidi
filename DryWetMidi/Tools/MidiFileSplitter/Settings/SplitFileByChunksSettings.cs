using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how a MIDI file should be split by chunks using
    /// <see cref="MidiFileSplitter.SplitByChunks(MidiFile, SplitFileByChunksSettings)"/> method.
    /// </summary>
    /// <seealso cref="MidiFileSplitter"/>
    public sealed class SplitFileByChunksSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a predicate to filter chunks out before processing. If predicate returns <c>true</c>,
        /// a chunk will be processed; if <c>false</c> - it won't. If the property set to <c>null</c> (default
        /// value), all MIDI chunks will be processed.
        /// </summary>
        public Predicate<MidiChunk> Filter { get; set; }

        #endregion
    }
}
