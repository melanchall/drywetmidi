using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class ChordsSplitterTests : LengthedObjectsSplitterTests<Chord>
    {
        #region Constructor

        public ChordsSplitterTests()
            : base(new ChordMethods(), new ChordsSplitter())
        {
        }

        #endregion

        #region Overrides

        protected override IEnumerable<Chord> CreateInputObjects(long length)
        {
            return length == 0
                ? new[]
                {
                    new Chord(new Note((SevenBitNumber)100),
                              new Note((SevenBitNumber)23)),
                    new Chord(new Note((SevenBitNumber)1),
                              new Note((SevenBitNumber)2),
                              new Note((SevenBitNumber)3))
                }
                : new[]
                {
                    new Chord(new Note((SevenBitNumber)100, length - 10),
                              new Note((SevenBitNumber)23, length - 10, 10)),
                    new Chord(new Note((SevenBitNumber)1, length - 1, 10),
                              new Note((SevenBitNumber)2, length - 5, 11),
                              new Note((SevenBitNumber)3, length - 10, 20))
                };
        }

        protected override SplittedLengthedObject<Chord> SplitObject(Chord obj, long time)
        {
            return obj.Split(time);
        }

        #endregion
    }
}
