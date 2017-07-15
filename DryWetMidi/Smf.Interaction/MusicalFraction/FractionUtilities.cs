using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class FractionUtilities
    {
        #region Methods

        internal static Fraction ToMathFraction(this IEnumerable<MusicalFractionCount> fractionsCounts)
        {
            if (fractionsCounts == null)
                throw new ArgumentNullException(nameof(fractionsCounts));

            return Fraction.Sum(fractionsCounts.Where(c => c != null && c.Fraction != null && c.Count > 0)
                                               .Select(ToMathFraction));
        }

        internal static Fraction FromTicks(long ticks, short ticksPerQuarterNote)
        {
            var xy = MathUtilities.SolveDiophantineEquation(4 * ticksPerQuarterNote, -ticks);
            return new Fraction(Math.Abs(xy.Item1), Math.Abs(xy.Item2));
        }

        internal static long ToTicks(Fraction fraction, short ticksPerQuarterNote)
        {
            return 4 * fraction.Numerator * ticksPerQuarterNote / fraction.Denominator;
        }

        private static Fraction ToMathFraction(MusicalFractionCount fractionsCount)
        {
            var fraction = fractionsCount.Fraction;

            int numerator = fractionsCount.Count;
            int denominator = fraction.Fraction;

            if (fraction.Dotted)
            {
                numerator *= 3;
                denominator *= 2;
            }

            var tuplet = fraction.Tuplet;
            if (tuplet != null)
            {
                numerator *= tuplet.SpaceSize;
                denominator *= tuplet.NotesCount;
            }

            return new Fraction(numerator, denominator);
        }

        #endregion
    }
}
