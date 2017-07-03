using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents time signature which is number of beats of specified length.
    /// </summary>
    public sealed class TimeSignature
    {
        #region Constants

        /// <summary>
        /// Default time signature which is 4/4.
        /// </summary>
        public static readonly TimeSignature Default = new TimeSignature(TimeSignatureEvent.DefaultNumerator,
                                                                         TimeSignatureEvent.DefaultDenominator);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSignature"/> with the specified
        /// numerator and denominator.
        /// </summary>
        /// <param name="numerator">Numerator of the time signature which defines number of beats.</param>
        /// <param name="denominator">Denominator of the time signature which defines beat length.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="numerator"/> is zero or negative. -or-
        /// <paramref name="denominator"/> is zero or negative.</exception>
        /// <exception cref="ArgumentException"><paramref name="denominator"/> is not a power of two.</exception>
        public TimeSignature(int numerator, int denominator)
        {
            if (numerator <= 0)
                throw new ArgumentOutOfRangeException("Numerator is zero or negative.", numerator, nameof(numerator));

            if (denominator <= 0)
                throw new ArgumentOutOfRangeException("Denominator is zero or negative.", denominator, nameof(denominator));

            if (!NumberUtilities.IsPowerOfTwo(denominator))
                throw new ArgumentException("Denominator is not a power of two.", nameof(denominator));

            Numerator = numerator;
            Denominator = denominator;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets numerator of the time signature which defines number of beats.
        /// </summary>
        public int Numerator { get; }

        /// <summary>
        /// Gets denominator of the time signature which defines beat length.
        /// </summary>
        public int Denominator { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Numerator}/{Denominator}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var timeSignature = obj as TimeSignature;
            if (ReferenceEquals(null, timeSignature))
                return false;

            return Numerator == timeSignature.Numerator &&
                   Denominator == timeSignature.Denominator;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Numerator.GetHashCode() ^ Denominator.GetHashCode();
        }

        #endregion
    }
}
