using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MusicalFractionUtilities
    {
        #region Methods

        internal static IEnumerable<MusicalFractionCount> Simplify(this IEnumerable<MusicalFractionCount> fractionsCounts)
        {
            if (fractionsCounts == null)
                throw new ArgumentNullException(nameof(fractionsCounts));

            return fractionsCounts.Where(c => c != null && c.Fraction != null && c.Count > 0)
                                  .GroupBy(c => c.Fraction)
                                  .Select(g => new MusicalFractionCount(g.Key, g.Sum(c => c.Count)))
                                  .ToList();
        }

        internal static void Equalize(IEnumerable<MusicalFractionCount> fractionsCounts1, IEnumerable<MusicalFractionCount> fractionsCounts2,
                                      out int numerator1, out int numerator2)
        {
            if (fractionsCounts1 == null)
                throw new ArgumentNullException(nameof(fractionsCounts1));

            if (fractionsCounts2 == null)
                throw new ArgumentNullException(nameof(fractionsCounts2));

            var mathFraction1 = fractionsCounts1.ToMathFraction();
            var mathFraction2 = fractionsCounts2.ToMathFraction();

            var commonDeniminator = NumberUtilities.LeastCommonMultiple(mathFraction1.Denominator, mathFraction2.Denominator);
            numerator1 = mathFraction1.Numerator * commonDeniminator / mathFraction1.Denominator;
            numerator2 = mathFraction2.Numerator * commonDeniminator / mathFraction2.Denominator;
        }

        internal static Fraction ToMathFraction(this IEnumerable<MusicalFractionCount> fractionsCounts)
        {
            if (fractionsCounts == null)
                throw new ArgumentNullException(nameof(fractionsCounts));

            return Fraction.Sum(fractionsCounts.Select(ToMathFraction));
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
