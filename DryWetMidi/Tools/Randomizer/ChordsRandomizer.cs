using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which chords should be randomized.
    /// </summary>
    public sealed class ChordsRandomizingSettings : LengthedObjectsRandomizingSettings<Chord>
    {
    }

    /// <summary>
    /// Provides methods to randomize chords time.
    /// </summary>
    public sealed class ChordsRandomizer : LengthedObjectsRandomizer<Chord, ChordsRandomizingSettings>
    {
    }
}
