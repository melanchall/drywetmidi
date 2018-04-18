using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed class NotesRandomizerTests : LengthedObjectsRandomizerTests<Note, NotesRandomizingSettings>
    {
        #region Properties

        protected override LengthedObjectsRandomizer<Note, NotesRandomizingSettings> Randomizer { get; } = new NotesRandomizer();

        protected override LengthedObjectMethods<Note> Methods { get; } = new NoteMethods();

        #endregion
    }
}
