using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class ChordsSplitterTests : LengthedObjectsSplitterTests<Chord>
    {
        #region Overrides

        protected override LengthedObjectsSplitter<Chord> Splitter { get; } = new ChordsSplitter();

        protected override LengthedObjectMethods<Chord> Methods { get; } = new ChordMethods();

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

        protected override Tuple<Chord, Chord> SplitObject(Chord obj, long time)
        {
            return obj.Split(time);
        }

        #endregion
    }
}
