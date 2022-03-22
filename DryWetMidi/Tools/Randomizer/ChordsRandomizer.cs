using Melanchall.DryWetMidi.Interaction;
using System;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which chords should be randomized.
    /// </summary>
    [Obsolete("OBS10")]
    public sealed class ChordsRandomizingSettings : LengthedObjectsRandomizingSettings<Chord>
    {
        #region Properties

        /// <summary>
        /// Gets or sets settings which define how chords should be detected and built.
        /// </summary>
        public ChordDetectionSettings ChordDetectionSettings { get; set; } = new ChordDetectionSettings();

        #endregion
    }

    /// <summary>
    /// Provides methods to randomize chords time.
    /// </summary>
    [Obsolete("OBS10")]
    public sealed class ChordsRandomizer : LengthedObjectsRandomizer<Chord, ChordsRandomizingSettings>
    {
    }
}
