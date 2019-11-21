using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class ThrowIfTimeArgument
    {
        #region Methods

        internal static void IsNegative(string parameterName, long time)
        {
            ThrowIfArgument.IsNegative(parameterName, time, "Time is negative.");
        }

        internal static void StartIsNegative(string parameterName, long time)
        {
            ThrowIfArgument.IsNegative(parameterName, time, "Start time is negative.");
        }

        internal static void EndIsNegative(string parameterName, long time)
        {
            ThrowIfArgument.IsNegative(parameterName, time, "End time is negative.");
        }

        #endregion
    }
}
