using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed class ChordsQuantizerTests : LengthedObjectsQuantizerTests<Chord, ChordsQuantizingSettings>
    {
        #region Properties

        protected override LengthedObjectsQuantizer<Chord, ChordsQuantizingSettings> Quantizer { get; } = new ChordsQuantizer();

        protected override LengthedObjectMethods<Chord> Methods { get; } = new ChordMethods();

        #endregion
    }
}
