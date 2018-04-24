using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class ChordsQuantizingSettings : LengthedObjectsQuantizingSettings
    {
    }

    public sealed class ChordsQuantizer : LengthedObjectsQuantizer<Chord, ChordsQuantizingSettings>
    {
    }
}
