using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which chords should be quantized.
    /// </summary>
    public class ChordsQuantizingSettings : LengthedObjectsQuantizingSettings<Chord>
    {
        #region Properties

        /// <summary>
        /// Gets or sets settings which define how chords should be detected and built.
        /// </summary>
        public ChordDetectionSettings ChordDetectionSettings { get; set; } = new ChordDetectionSettings();

        #endregion
    }

    /// <summary>
    /// Provides methods to quantize chords time.
    /// </summary>
    /// <remarks>
    /// See <see href="xref:a_quantizer">Quantizer</see> article to learn more.
    /// </remarks>
    public class ChordsQuantizer : LengthedObjectsQuantizer<Chord, ChordsQuantizingSettings>
    {
    }
}
