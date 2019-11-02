using System;
using System.Collections;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    public sealed class ChordMethods : LengthedObjectMethods<Chord>
    {
        #region Nested classes

        private sealed class ChordComparer : IComparer
        {
            #region IComparer

            public int Compare(object x, object y)
            {
                var xChord = (Chord)x;
                var yChord = (Chord)y;

                return ChordEquality.AreEqual(xChord, yChord) ? 0 : -1;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly Random _random = new Random();

        #endregion

        #region Overrides

        protected override IComparer Comparer { get; } = new ChordComparer();

        public override void SetTime(Chord obj, long time)
        {
            obj.Time = time;
        }

        public override void SetLength(Chord obj, long length)
        {
            obj.Length = length;
        }

        public override Chord Create(long time, long length)
        {
            var chord = new Chord(new Note((SevenBitNumber)_random.Next(SevenBitNumber.MaxValue)),
                                  new Note((SevenBitNumber)_random.Next(SevenBitNumber.MaxValue)));
            chord.Time = time;
            chord.Length = length;

            return chord;
        }

        public override Chord Clone(Chord obj)
        {
            return obj.Clone();
        }

        #endregion
    }
}
