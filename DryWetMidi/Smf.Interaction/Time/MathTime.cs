using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MathTime : ITime
    {
        #region Constructor

        public MathTime(ITime time, ILength offset, MathOperation operation = MathOperation.Sum)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(offset), offset);

            Time = time;
            Offset = offset;
            Operation = operation;
        }

        #endregion

        #region Properties

        public ITime Time { get; }

        public ILength Offset { get; }

        public MathOperation Operation { get; }

        #endregion
    }
}
