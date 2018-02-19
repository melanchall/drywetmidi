using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class IntervalUtilities
    {
        #region Methods

        internal static bool IsIntervalValid(int halfSteps)
        {
            return halfSteps >= -SevenBitNumber.MaxValue && halfSteps <= SevenBitNumber.MaxValue;
        }

        #endregion
    }
}
