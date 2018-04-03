using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class NotesQuantizingSettings : LengthedObjectsQuantizingSettings
    {
    }

    public sealed class NotesQuantizer : LengthedObjectsQuantizer<Note, NotesQuantizingSettings>
    {
        #region Overrides

        protected override void SetObjectTime(Note obj, long time)
        {
            obj.Time = time;
        }

        protected override void SetObjectLength(Note obj, long length)
        {
            obj.Length = length;
        }

        #endregion
    }
}
