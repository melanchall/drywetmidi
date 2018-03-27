using System;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class NotesSplitter : LengthedObjectsSplitter<Note>
    {
        #region Overrides

        protected override Note CloneObject(Note obj)
        {
            return obj.Clone();
        }

        protected override Tuple<Note, Note> SplitObject(Note obj, long time)
        {
            return obj.Split(time);
        }

        #endregion
    }
}
