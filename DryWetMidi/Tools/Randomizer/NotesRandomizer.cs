using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which notes should be randomized.
    /// </summary>
    public sealed class NotesRandomizingSettings : LengthedObjectsRandomizingSettings<Note>
    {
    }

    /// <summary>
    /// Provides methods to randomize notes time.
    /// </summary>
    /// <remarks>
    /// See <see href="xref:a_randomizer">Randomizer</see> article on Wiki to learn more.
    /// </remarks>
    public sealed class NotesRandomizer : LengthedObjectsRandomizer<Note, NotesRandomizingSettings>
    {
    }
}
