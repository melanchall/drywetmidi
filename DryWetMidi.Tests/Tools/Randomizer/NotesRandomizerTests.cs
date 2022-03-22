using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using System;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [Obsolete("OBS10")]
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
