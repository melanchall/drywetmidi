using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class ChordsSplitterTests : LengthedObjectsSplitterTests<Chord>
    {
        #region Nestd classes

        private sealed class ChordComparer : System.Collections.IComparer
        {
            #region IComparer

            public int Compare(object x, object y)
            {
                var xChord = (Chord)x;
                var yChord = (Chord)y;

                return ChordEquality.Equals(xChord, yChord) ? 0 : -1;
            }

            #endregion
        }

        #endregion

        #region Overrides

        protected override LengthedObjectsSplitter<Chord> Splitter { get; } = new ChordsSplitter();

        protected override System.Collections.IComparer Comparer { get; } = new ChordComparer();

        protected override Chord CloneObject(Chord obj)
        {
            return obj.Clone();
        }

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

        protected override Chord CreateObject(long time, long length)
        {
            var chord = new Chord(new Note((SevenBitNumber)34),
                                  new Note((SevenBitNumber)35));
            chord.Time = time;
            chord.Length = length;

            return chord;
        }

        protected override Tuple<Chord, Chord> SplitObject(Chord obj, long time)
        {
            return obj.Split(time);
        }

        protected override void SetObjectTimeAndLength(Chord obj, long time, long length)
        {
            obj.Time = time;
            obj.Length = length;
        }

        #endregion
    }
}
