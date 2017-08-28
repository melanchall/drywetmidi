using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class LengthUtilities
    {
        #region Methods

        public static ILength Add(this ILength length1, ILength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return new MathLength(length1, length2, MathOperation.Add);
        }

        public static ILength Subtract(this ILength length1, ILength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return new MathLength(length1, length2, MathOperation.Subtract);
        }

        public static ITime ToTime(this ILength length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            return ((MidiTime)0).Add(length);
        }

        #endregion
    }
}
