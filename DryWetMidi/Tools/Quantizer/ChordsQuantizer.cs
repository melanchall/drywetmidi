using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which chords should be quantized.
    /// </summary>
    public class ChordsQuantizingSettings : LengthedObjectsQuantizingSettings
    {
    }

    /// <summary>
    /// Provides methods to quantize chords time.
    /// </summary>
    public class ChordsQuantizer : LengthedObjectsQuantizer<Chord, ChordsQuantizingSettings>
    {
    }
}
