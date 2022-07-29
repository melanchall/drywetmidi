using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    public sealed class NoteMethods : LengthedObjectMethods<Note>
    {
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
            return new Note((SevenBitNumber)DryWetMidi.Common.Random.Instance.Next(SevenBitNumber.MaxValue), length, time);
        }

        #endregion
    }
}
