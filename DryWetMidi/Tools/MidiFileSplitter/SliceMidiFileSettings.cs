using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which a <see cref="MidiFile"/> should be splitted.
    /// </summary>
    public class SliceMidiFileSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether notes should be splitted in points of
        /// grid intersection or not. The default value is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// False means notes treated as just Note On / Note Off events rather than note objects
        /// for <c>true</c>. Splitting notes produces new Note On / Note Off events at points of grid
        /// intersecting notes.
        /// </remarks>
        public bool SplitNotes { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether original times of events should be saved or not.
        /// The default value is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// If <c>false</c> used, events will be moved to the start of a new file. If <c>true</c> used, events
        /// will be placed in new files at the same times as in the input file.
        /// </remarks>
        public bool PreserveTimes { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether track chunks in new files should correspond
        /// to those in the input file or not, so empty track chunks can be presented in new files.
        /// The default value is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// If <c>false</c> used, track chunks without events will be removed from the result.
        /// </remarks>
        public bool PreserveTrackChunks { get; set; } = false;

        public SliceMidiFileMarkers Markers { get; set; }

        #endregion
    }
}
