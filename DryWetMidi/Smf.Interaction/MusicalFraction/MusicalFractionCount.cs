using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MusicalFractionCount
    {
        #region Constants

        private const int DefaultCount = 1;

        #endregion

        #region Constructor

        public MusicalFractionCount()
            : this(new MusicalFraction())
        {
        }

        public MusicalFractionCount(MusicalFraction fraction)
            : this(fraction, DefaultCount)
        {
        }

        public MusicalFractionCount(MusicalFraction fraction, int count)
        {
            if (fraction == null)
                throw new ArgumentNullException(nameof(fraction));

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count is zero or negative.");

            Fraction = fraction;
            Count = count;
        }

        #endregion

        #region Properties

        public MusicalFraction Fraction { get; }

        public int Count { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="fractionCount">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(MusicalFractionCount fractionCount)
        {
            return this == fractionCount;
        }

        #endregion

        #region Operators

        public static bool operator ==(MusicalFractionCount fractionCount1, MusicalFractionCount fractionCount2)
        {
            if (ReferenceEquals(fractionCount1, fractionCount2))
                return true;

            if (ReferenceEquals(null, fractionCount1) || ReferenceEquals(null, fractionCount2))
                return false;

            return fractionCount1.Fraction == fractionCount2.Fraction &&
                   fractionCount1.Count == fractionCount2.Count;
        }

        public static bool operator !=(MusicalFractionCount fractionCount1, MusicalFractionCount fractionCount2)
        {
            return !(fractionCount1 == fractionCount2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Count}x{Fraction}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as MusicalFractionCount);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Fraction.GetHashCode() ^ Count.GetHashCode();
        }

        #endregion
    }
}
