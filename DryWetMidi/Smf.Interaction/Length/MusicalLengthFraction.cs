using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MusicalLengthFraction
    {
        #region Constants

        public static readonly MusicalLengthFraction Whole = new MusicalLengthFraction(WholeFraction);
        public static readonly MusicalLengthFraction WholeDotted = new MusicalLengthFraction(WholeFraction, true);
        public static readonly MusicalLengthFraction WholeTriplet = new MusicalLengthFraction(WholeFraction, TupletDefinition.Triplet);

        public static readonly MusicalLengthFraction Half = new MusicalLengthFraction(HalfFraction);
        public static readonly MusicalLengthFraction HalfDotted = new MusicalLengthFraction(HalfFraction, true);
        public static readonly MusicalLengthFraction HalfTriplet = new MusicalLengthFraction(HalfFraction, TupletDefinition.Triplet);

        public static readonly MusicalLengthFraction Quarter = new MusicalLengthFraction(QuarterFraction);
        public static readonly MusicalLengthFraction QuarterDotted = new MusicalLengthFraction(QuarterFraction, true);
        public static readonly MusicalLengthFraction QuarterTriplet = new MusicalLengthFraction(QuarterFraction, TupletDefinition.Triplet);

        public static readonly MusicalLengthFraction Eighth = new MusicalLengthFraction(EighthFraction);
        public static readonly MusicalLengthFraction EighthDotted = new MusicalLengthFraction(EighthFraction, true);
        public static readonly MusicalLengthFraction EighthTriplet = new MusicalLengthFraction(EighthFraction, TupletDefinition.Triplet);

        public static readonly MusicalLengthFraction Sixteenth = new MusicalLengthFraction(SixteenthFraction);
        public static readonly MusicalLengthFraction SixteenthDotted = new MusicalLengthFraction(SixteenthFraction, true);
        public static readonly MusicalLengthFraction SixteenthTriplet = new MusicalLengthFraction(SixteenthFraction, TupletDefinition.Triplet);

        public static readonly MusicalLengthFraction ThirtySecond = new MusicalLengthFraction(ThirtySecondFraction);
        public static readonly MusicalLengthFraction ThirtySecondDotted = new MusicalLengthFraction(ThirtySecondFraction, true);
        public static readonly MusicalLengthFraction ThirtySecondTriplet = new MusicalLengthFraction(ThirtySecondFraction, TupletDefinition.Triplet);

        public static readonly MusicalLengthFraction SixtyFourth = new MusicalLengthFraction(SixtyFourthFraction);
        public static readonly MusicalLengthFraction SixtyFourthDotted = new MusicalLengthFraction(SixtyFourthFraction, true);
        public static readonly MusicalLengthFraction SixtyFourthTriplet = new MusicalLengthFraction(SixtyFourthFraction, TupletDefinition.Triplet);

        private const int WholeFraction = 1;
        private const int HalfFraction = 2;
        private const int QuarterFraction = 4;
        private const int EighthFraction = 8;
        private const int SixteenthFraction = 16;
        private const int ThirtySecondFraction = 32;
        private const int SixtyFourthFraction = 64;

        #endregion

        #region Constructor

        public MusicalLengthFraction(int fraction)
            : this(fraction, false, null)
        {
        }

        public MusicalLengthFraction(int fraction, bool dotted)
            : this(fraction, dotted, null)
        {
        }

        public MusicalLengthFraction(int fraction, TupletDefinition tupletDefinition)
            : this(fraction, false, tupletDefinition)
        {
        }

        public MusicalLengthFraction(int fraction, bool dotted, TupletDefinition tupletDefinition)
        {
            if (fraction <= 0)
                throw new ArgumentOutOfRangeException(nameof(fraction), fraction, "Fraction is zero or negative.");

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
        public bool Equals(MusicalLengthFraction fraction)
        {
            if (ReferenceEquals(null, fraction))
                return false;

            if (ReferenceEquals(this, fraction))
                return true;

            return Fraction == fraction.Fraction &&
                   Dotted == fraction.Dotted &&
                   (Tuplet?.Equals(fraction.Tuplet) ?? ReferenceEquals(null, fraction.Tuplet));
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
            return Equals(obj as MusicalLengthFraction);
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
