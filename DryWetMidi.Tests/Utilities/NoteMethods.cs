using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    public sealed class NoteMethods : LengthedObjectMethods<Note>
    {
        #region Fields

        private readonly Random _random = new Random();

        #endregion

        #region Overrides

        public override void SetTime(Note obj, long time)
        {
            obj.Time = time;
        }

        public override void SetLength(Note obj, long length)
        {
            obj.Length = length;
        }

        public override Note Create(long time, long length)
        {
            return new Note((SevenBitNumber)_random.Next(SevenBitNumber.MaxValue), length, time);
        }

        #endregion
    }
}
