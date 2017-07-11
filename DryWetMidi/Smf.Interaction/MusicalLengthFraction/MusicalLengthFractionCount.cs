using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MusicalLengthFractionCount
    {
        #region Constructor

        public MusicalLengthFractionCount(MusicalLengthFraction fraction, int count)
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

        public MusicalLengthFraction Fraction { get; }

        public int Count { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="fractionCount">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(MusicalLengthFractionCount fractionCount)
        {
            if (ReferenceEquals(null, fractionCount))
                return false;

            if (ReferenceEquals(this, fractionCount))
                return true;

            return Fraction.Equals(fractionCount.Fraction) &&
                   Count == fractionCount.Count;
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
            return Equals(obj as MusicalLengthFractionCount);
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
