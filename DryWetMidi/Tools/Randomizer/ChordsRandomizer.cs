using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class ChordsRandomizingSettings : LengthedObjectsRandomizingSettings
    {
    }

    public sealed class ChordsRandomizer : LengthedObjectsRandomizer<Chord, ChordsRandomizingSettings>
    {
        #region Overrides

        protected override void SetObjectTime(Chord obj, long time)
        {
            obj.Time = time;
        }

        protected override void SetObjectLength(Chord obj, long length)
        {
            obj.Length = length;
        }

        #endregion
    }
}
