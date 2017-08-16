using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
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
