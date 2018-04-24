using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class NotesRandomizingSettings : LengthedObjectsRandomizingSettings
    {
    }

    public sealed class NotesRandomizer : LengthedObjectsRandomizer<Note, NotesRandomizingSettings>
    {
    }
}
