using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class NotesQuantizingSettings : LengthedObjectsQuantizingSettings
    {
    }

    public sealed class NotesQuantizer : LengthedObjectsQuantizer<Note, NotesQuantizingSettings>
    {
    }
}
