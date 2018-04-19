using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed class ChordsQuantizerTests : LengthedObjectsQuantizerTests<Chord, ChordsQuantizingSettings>
    {
        #region Constructor

        public ChordsQuantizerTests()
            : base(new ChordMethods(), new ChordsQuantizer())
        {
        }

        #endregion
    }
}
