using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MusicalLengthFractionUtilities
    {
        #region Methods

        internal static IEnumerable<MusicalLengthFractionCount> Simplify(this IEnumerable<MusicalLengthFractionCount> fractionsCounts)
        {
            if (fractionsCounts == null)
                throw new ArgumentNullException(nameof(fractionsCounts));

            return fractionsCounts.Where(c => c != null && c.Fraction != null && c.Count > 0)
                                  .GroupBy(c => c.Fraction)
                                  .Select(g => new MusicalLengthFractionCount(g.Key, g.Sum(c => c.Count)))
                                  .ToList();
        }

        internal static long ToTicks(this IEnumerable<MusicalLengthFractionCount> fractionsCounts, short ticksPerQuarterNote)
        {
            if (fractionsCounts == null)
                throw new ArgumentNullException(nameof(fractionsCounts));

            return fractionsCounts.Sum(c => GetFractionLength(c.Fraction, ticksPerQuarterNote, c.Count));
        }

        private static long GetFractionLength(MusicalLengthFraction fraction, short ticksPerQuarterNote, int multiplier = 1)
        {
            var baseLength = fraction.Dotted
                ? (multiplier * 6 * ticksPerQuarterNote / fraction.Fraction)
                : (multiplier * 4 * ticksPerQuarterNote / fraction.Fraction);

            var tuplet = fraction.Tuplet;
            return tuplet == null
                ? baseLength
                : (multiplier * tuplet.SpaceSize * baseLength / tuplet.NotesCount);
        }

        #endregion
    }
}
