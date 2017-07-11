using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MusicalLength : ILength
    {
        #region Constructor

        public MusicalLength(MusicalLengthFraction fraction)
            : this(fraction, 1)
        {
        }

        public MusicalLength(MusicalLengthFraction fraction, int fractionCount)
        {
            if (fraction == null)
                throw new ArgumentNullException(nameof(fraction));

            if (fractionCount < 0)
                throw new ArgumentOutOfRangeException(nameof(fractionCount), fractionCount, "Fraction count is negative.");

            Fractions = new[] { new MusicalLengthFractionCount(fraction, fractionCount) };
        }

        public MusicalLength(params MusicalLengthFractionCount[] fractionsCounts)
            : this((IEnumerable<MusicalLengthFractionCount>)fractionsCounts)
        {
        }

        public MusicalLength(IEnumerable<MusicalLengthFractionCount> fractionsCounts)
        {
            if (fractionsCounts == null)
                throw new ArgumentNullException(nameof(fractionsCounts));

            Fractions = fractionsCounts.Simplify();
        }

        #endregion

        #region Properties

        public IEnumerable<MusicalLengthFractionCount> Fractions { get; } = Enumerable.Empty<MusicalLengthFractionCount>();

        #endregion

        #region Operators

        public static implicit operator MusicalLength(MusicalLengthFraction fraction)
        {
            return new MusicalLength(fraction);
        }

        public static implicit operator MusicalLength(MusicalLengthFractionCount fractionCount)
        {
            return new MusicalLength(fractionCount);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return string.Join(" + ", Fractions);
        }

        #endregion
    }
}
