using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class NotesQuantizerTests : LengthedObjectsQuantizerTests<Note, NotesQuantizingSettings>
    {
        #region Constructor

        public NotesQuantizerTests()
            : base(new NoteMethods(), new NotesQuantizer())
        {
        }

        #endregion
    }
}
