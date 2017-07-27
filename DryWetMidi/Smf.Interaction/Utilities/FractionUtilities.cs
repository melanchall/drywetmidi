using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class FractionUtilities
    {
        #region Methods

        internal static Fraction FromTicks(long ticks, short ticksPerQuarterNote)
        {
            var xy = MathUtilities.SolveDiophantineEquation(4 * ticksPerQuarterNote, -ticks);
            return new Fraction(Math.Abs(xy.Item1), Math.Abs(xy.Item2));
        }

        internal static long ToTicks(this Fraction fraction, short ticksPerQuarterNote)
        {
            return 4 * fraction.Numerator * ticksPerQuarterNote / fraction.Denominator;
        }

        #endregion
    }
}
