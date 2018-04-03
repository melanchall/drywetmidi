using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class ChordsQuantizingSettings : LengthedObjectsQuantizingSettings
    {
    }

    public sealed class ChordsQuantizer : LengthedObjectsQuantizer<Chord, ChordsQuantizingSettings>
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
