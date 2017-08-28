using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TimeUtilities
    {
        #region Methods

        public static ITime Add(this ITime time, ILength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

            return new MathTime(time, length, MathOperation.Add);
        }

        public static ITime Subtract(this ITime time, ILength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

            return new MathTime(time, length, MathOperation.Subtract);
        }

        #endregion
    }
}
