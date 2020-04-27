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
    /// <remarks>
    /// See <see href="xref:a_randomizer">Randomizer</see> article on Wiki to learn more.
    /// </remarks>
    public sealed class ChordsRandomizer : LengthedObjectsRandomizer<Chord, ChordsRandomizingSettings>
    {
    }
}
