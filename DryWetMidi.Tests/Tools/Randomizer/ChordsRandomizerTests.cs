using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
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
