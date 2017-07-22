using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MusicalFraction
    {
        #region Constants

        public static readonly MusicalFraction Whole = new MusicalFraction(WholeFraction);
        public static readonly MusicalFraction WholeDotted = new MusicalFraction(WholeFraction, true);
        public static readonly MusicalFraction WholeTriplet = new MusicalFraction(WholeFraction, TupletDefinition.Triplet);

        public static readonly MusicalFraction Half = new MusicalFraction(HalfFraction);
        public static readonly MusicalFraction HalfDotted = new MusicalFraction(HalfFraction, true);
        public static readonly MusicalFraction HalfTriplet = new MusicalFraction(HalfFraction, TupletDefinition.Triplet);

        public static readonly MusicalFraction Quarter = new MusicalFraction(QuarterFraction);
        public static readonly MusicalFraction QuarterDotted = new MusicalFraction(QuarterFraction, true);
        public static readonly MusicalFraction QuarterTriplet = new MusicalFraction(QuarterFraction, TupletDefinition.Triplet);

        public static readonly MusicalFraction Eighth = new MusicalFraction(EighthFraction);
        public static readonly MusicalFraction EighthDotted = new MusicalFraction(EighthFraction, true);
        public static readonly MusicalFraction EighthTriplet = new MusicalFraction(EighthFraction, TupletDefinition.Triplet);

        public static readonly MusicalFraction Sixteenth = new MusicalFraction(SixteenthFraction);
        public static readonly MusicalFraction SixteenthDotted = new MusicalFraction(SixteenthFraction, true);
        public static readonly MusicalFraction SixteenthTriplet = new MusicalFraction(SixteenthFraction, TupletDefinition.Triplet);

        public static readonly MusicalFraction ThirtySecond = new MusicalFraction(ThirtySecondFraction);
        public static readonly MusicalFraction ThirtySecondDotted = new MusicalFraction(ThirtySecondFraction, true);
        public static readonly MusicalFraction ThirtySecondTriplet = new MusicalFraction(ThirtySecondFraction, TupletDefinition.Triplet);

        public static readonly MusicalFraction SixtyFourth = new MusicalFraction(SixtyFourthFraction);
        public static readonly MusicalFraction SixtyFourthDotted = new MusicalFraction(SixtyFourthFraction, true);
        public static readonly MusicalFraction SixtyFourthTriplet = new MusicalFraction(SixtyFourthFraction, TupletDefinition.Triplet);

        private const int WholeFraction = 1;
        private const int HalfFraction = 2;
        private const int QuarterFraction = 4;
        private const int EighthFraction = 8;
        private const int SixteenthFraction = 16;
        private const int ThirtySecondFraction = 32;
        private const int SixtyFourthFraction = 64;

        #endregion

        #region Constructor

        public MusicalFraction()
            : this(WholeFraction, false, null)
        {
        }

        public MusicalFraction(int fraction)
            : this(fraction, false, null)
        {
        }

        public MusicalFraction(int fraction, bool dotted)
            : this(fraction, dotted, null)
        {
        }

        public MusicalFraction(int fraction, TupletDefinition tupletDefinition)
            : this(fraction, false, tupletDefinition)
        {
        }

        public MusicalFraction(int fraction, bool dotted, TupletDefinition tupletDefinition)
        {
            ThrowIfArgument.IsNonpositive(nameof(fraction), fraction, "Fraction is zero or negative.");

            Fraction = fraction;
            Dotted = dotted;
            Tuplet = tupletDefinition;
        }

        #endregion

        #region Properties

        public int Fraction { get; }

        public bool Dotted { get; }

        public TupletDefinition Tuplet { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="fraction">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(MusicalFraction fraction)
        {
            return this == fraction;
        }

        #endregion

        #region Operators

        public static bool operator==(MusicalFraction fraction1, MusicalFraction fraction2)
        {
            if (ReferenceEquals(fraction1, fraction2))
                return true;

            if (ReferenceEquals(null, fraction1) || ReferenceEquals(null, fraction2))
                return false;

            return fraction1.Fraction == fraction2.Fraction &&
                   fraction1.Dotted == fraction2.Dotted &&
                   fraction1.Tuplet == fraction2.Tuplet;
        }

        public static bool operator !=(MusicalFraction fraction1, MusicalFraction fraction2)
        {
            return !(fraction1 == fraction2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Fraction}{(Dotted ? "." : string.Empty)}{(Tuplet != null ? $" {Tuplet}" : string.Empty)}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as MusicalFraction);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Fraction.GetHashCode() ^ Dotted.GetHashCode() ^ (Tuplet?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
