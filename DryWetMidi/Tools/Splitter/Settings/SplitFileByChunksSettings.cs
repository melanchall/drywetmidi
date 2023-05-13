using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how a MIDI file should be split by chunks using
    /// <see cref="Splitter.SplitByChunks(MidiFile, SplitFileByChunksSettings)"/> method.
    /// </summary>
    /// <seealso cref="Splitter"/>
    public sealed class SplitFileByChunksSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a predicate to filter chunks out before processing. If predicate returns <c>true</c>,
        /// a chunk will be processed; if <c>false</c> - it won't. If the property set to <c>null</c> (default
        /// value), all MIDI chunks will be processed.
        /// </summary>
        public Predicate<MidiChunk> Filter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a tempo map should be preserved or not in new files.
        /// The default value is <c>true</c>.
        /// </summary>
        public bool PreserveTempoMap { get; set; } = true;

        #endregion
    }
}
