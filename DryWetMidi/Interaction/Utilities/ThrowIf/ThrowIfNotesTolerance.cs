using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class ThrowIfNotesTolerance
    {
        #region Methods

        internal static void IsNegative(string parameterName, long notesTolerance)
        {
            ThrowIfArgument.IsNegative(parameterName, notesTolerance, "Notes tolerance is negative.");
        }

        #endregion
    }
}
