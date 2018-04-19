using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed class ChordsRandomizerTests : LengthedObjectsRandomizerTests<Chord, ChordsRandomizingSettings>
    {
        #region Constructor

        public ChordsRandomizerTests()
            : base(new ChordMethods(), new ChordsRandomizer())
        {
        }

        #endregion
    }
}
