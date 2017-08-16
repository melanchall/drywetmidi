using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class ThrowIfTimeDivision
    {
        #region Methods

        internal static void IsNotSupportedForTimeConversion(TimeDivision timeDivision)
        {
            throw new NotSupportedException($"{timeDivision} is not supported for time conversion.");
        }

        internal static void IsNotSupportedForLengthConversion(TimeDivision timeDivision)
        {
            throw new NotSupportedException($"{timeDivision} is not supported for length conversion.");
        }

        #endregion
    }
}
