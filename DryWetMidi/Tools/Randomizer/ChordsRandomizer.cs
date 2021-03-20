using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which chords should be randomized.
    /// </summary>
    public sealed class ChordsRandomizingSettings : LengthedObjectsRandomizingSettings<Chord>
    {
        #region Properties

        public ChordDetectionSettings ChordDetectionSettings { get; set; } = new ChordDetectionSettings();

        #endregion
    }

    /// <summary>
    /// Provides methods to randomize chords time.
    /// </summary>
    public sealed class ChordsRandomizer : LengthedObjectsRandomizer<Chord, ChordsRandomizingSettings>
    {
    }
}
