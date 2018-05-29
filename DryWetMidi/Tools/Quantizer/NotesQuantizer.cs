using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public class NotesQuantizingSettings : LengthedObjectsQuantizingSettings
    {
    }

    public class NotesQuantizer : LengthedObjectsQuantizer<Note, NotesQuantizingSettings>
    {
    }
}
