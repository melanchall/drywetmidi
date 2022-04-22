using Melanchall.DryWetMidi.Interaction;
using System;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which chords should be quantized.
    /// </summary>
    [Obsolete("OBS13")]
    public class ChordsQuantizingSettings : LengthedObjectsQuantizingSettings<Chord>
    {
        #region Properties

        /// <summary>
        /// Gets or sets settings which define how chords should be detected and built.
        /// </summary>
        public ChordDetectionSettings ChordDetectionSettings { get; set; } = new ChordDetectionSettings();

        #endregion
    }
}
