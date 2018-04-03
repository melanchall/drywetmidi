using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class NotesQuantizerTests : LengthedObjectsQuantizerTests<Note, NotesQuantizingSettings>
    {
        #region Properties

        protected override LengthedObjectsQuantizer<Note, NotesQuantizingSettings> Quantizer { get; } = new NotesQuantizer();

        protected override LengthedObjectMethods<Note> Methods { get; } = new NoteMethods();

        #endregion
    }
}
