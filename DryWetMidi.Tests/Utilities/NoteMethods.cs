using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    public sealed class NoteMethods : LengthedObjectMethods<Note>
    {
        #region Overrides

        public override Note Create(long time, long length)
        {
            return new Note((SevenBitNumber)Random.Instance.Next(SevenBitNumber.MaxValue), length, time);
        }

        #endregion
    }
}
