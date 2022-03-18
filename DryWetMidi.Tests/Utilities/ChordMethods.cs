using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    public sealed class ChordMethods : LengthedObjectMethods<Chord>
    {
        #region Fields

        private readonly Random _random = new Random();

        #endregion

        #region Overrides

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

        #endregion
    }
}
