using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MusicalLength : ILength
    {
        #region Constructor

        public MusicalLength(MusicalFraction fraction, int fractionCount)
        {
            ThrowIfArgument.IsNull(nameof(fraction), fraction);
            ThrowIfArgument.IsNegative(nameof(fractionCount), fractionCount, "Fraction count is negative.");

            Fraction = new[] { new MusicalFractionCount(fraction, fractionCount) }.ToMathFraction();
        }

        public MusicalLength(params MusicalFraction[] fractions)
            : this(fractions as IEnumerable<MusicalFraction>)
        {
        }

        public MusicalLength(IEnumerable<MusicalFraction> fractions)
            : this(fractions?.Select(f => new MusicalFractionCount(f, 1)))
        {
        }

        public MusicalLength(params MusicalFractionCount[] fractionsCounts)
            : this(fractionsCounts as IEnumerable<MusicalFractionCount>)
        {
        }

        public MusicalLength(IEnumerable<MusicalFractionCount> fractionsCounts)
        {
            ThrowIfArgument.IsNull(nameof(fractionsCounts), fractionsCounts);

            Fraction = fractionsCounts.ToMathFraction();
        }

        public MusicalLength(Fraction fraction)
        {
            ThrowIfArgument.IsNull(nameof(fraction), fraction);

            Fraction = fraction;
        }

        #endregion

        #region Properties

        public Fraction Fraction { get; }

        #endregion

        #region Methods

        public bool Equals(MusicalLength length)
        {
            if (ReferenceEquals(null, length))
                return false;

            if (ReferenceEquals(this, length))
                return true;

            return Fraction == length.Fraction;
        }

        #endregion

        #region Operators

        public static implicit operator MusicalLength(Fraction fraction)
        {
            return new MusicalLength(fraction);
        }

        public static implicit operator MusicalLength(MusicalFraction fraction)
        {
            return new MusicalLength(fraction);
        }

        public static implicit operator MusicalLength(MusicalFractionCount fractionCount)
        {
            return new MusicalLength(fractionCount);
        }

        public static implicit operator Fraction(MusicalLength length)
        {
            return length.Fraction;
        }

        public static MusicalLength operator +(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return new MusicalLength(length1.Fraction + length2.Fraction);
        }

        public static MusicalLength operator -(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            if (length1.Fraction < length2.Fraction)
                throw new ArgumentException("First length is less than second one.", nameof(length1));

            return new MusicalLength(length1.Fraction - length2.Fraction);
        }

        public static bool operator <(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Fraction < length2.Fraction;
        }

        public static bool operator >(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Fraction > length2.Fraction;
        }

        public static bool operator <=(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Fraction <= length2.Fraction;
        }

        public static bool operator >=(MusicalLength length1, MusicalLength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return length1.Fraction >= length2.Fraction;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as MusicalLength);
        }

        public override int GetHashCode()
        {
            return Fraction.GetHashCode();
        }

        public override string ToString()
        {
            return Fraction.ToString();
        }

        #endregion
    }
}
