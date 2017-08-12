using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MathTime : ITime
    {
        #region Constructor

        public MathTime(ITime time, ILength offset, MathOperation operation = MathOperation.Sum)
            : this(offset, operation)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            Time = time;
        }

        public MathTime(long time, ILength offset, MathOperation operation = MathOperation.Sum)
            : this(offset, operation)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

            MidiTime = time;
        }

        private MathTime(ILength offset, MathOperation operation = MathOperation.Sum)
        {
            ThrowIfArgument.IsNull(nameof(offset), offset);
            ThrowIfArgument.IsInvalidEnumValue(nameof(operation), operation);

            Offset = offset;
            Operation = operation;
        }

        #endregion

        #region Properties

        public ITime Time { get; }

        public long MidiTime { get; }

        public ILength Offset { get; }

        public MathOperation Operation { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            var operationString = Operation == MathOperation.Sum
                ? "+"
                : "-";

            return $"({Time ?? (object)MidiTime} {operationString} {Offset})";
        }

        #endregion
    }
}
