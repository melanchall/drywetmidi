using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Specifies settings which control how MIDI files should be merged "simultaneously".
    /// </summary>
    /// <seealso cref="Merger.MergeSimultaneously(System.Collections.Generic.IEnumerable{Core.MidiFile}, SimultaneousMergingSettings)"/>
    public sealed class SimultaneousMergingSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether non-track chunks (like <see cref="UnknownChunk"/> instances
        /// or <see href="xref:a_custom_chunk">custom ones</see>) should be copied to the result file
        /// or not. The default value is <c>true</c>.
        /// </summary>
        public bool CopyNonTrackChunks { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore different tempo maps of the input files
        /// or not. The default value is <c>false</c> which means an exception will be thrown if
        /// tempo maps are not the same.
        /// </summary>
        public bool IgnoreDifferentTempoMaps { get; set; }

        #endregion
    }
}
