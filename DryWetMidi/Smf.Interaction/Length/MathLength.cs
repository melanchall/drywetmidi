using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MathLength : ILength
    {
        #region Constructor

        public MathLength(ILength length1, ILength length2, MathOperation operation = MathOperation.Sum)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            Length1 = length1;
            Length2 = length2;
            Operation = operation;
        }

        #endregion

        #region Properties

        public ILength Length1 { get; }

        public ILength Length2 { get; }

        public MathOperation Operation { get; }

        #endregion
    }
}
