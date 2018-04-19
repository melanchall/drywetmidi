using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed class NotesRandomizerTests : LengthedObjectsRandomizerTests<Note, NotesRandomizingSettings>
    {
        #region Constructor

        public NotesRandomizerTests()
            : base(new NoteMethods(), new NotesRandomizer())
        {
        }

        #endregion
    }
}
