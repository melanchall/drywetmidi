using System;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class ChordsSplitter : LengthedObjectsSplitter<Chord>
    {
        #region Overrides

        protected override Chord CloneObject(Chord obj)
        {
            return obj.Clone();
        }

        protected override Tuple<Chord, Chord> SplitObject(Chord obj, long time)
        {
            return obj.Split(time);
        }

        #endregion
    }
}
