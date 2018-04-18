using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class NotesRandomizingSettings : LengthedObjectsRandomizingSettings
    {
    }

    public sealed class NotesRandomizer : LengthedObjectsRandomizer<Note, NotesRandomizingSettings>
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
