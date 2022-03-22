using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using System;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [Obsolete("OBS10")]
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
