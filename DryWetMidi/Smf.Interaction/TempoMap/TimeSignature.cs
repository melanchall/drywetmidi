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
        /// <paramref name="denominator"/> is zero or negative. -or- <paramref name="denominator"/> is not a
        /// power of two.</exception>
        public TimeSignature(int numerator, int denominator)
        {
            ThrowIfArgument.IsNonpositive(nameof(numerator), numerator, "Numerator is zero or negative.");
            ThrowIfArgument.IsNonpositive(nameof(denominator), denominator, "Denominator is zero or negative.");
            ThrowIfArgument.DoesntSatisfyCondition(nameof(denominator),
                                                   denominator,
                                                   MathUtilities.IsPowerOfTwo,
                                                   "Denominator is not a power of two.");

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

        #region Operators

        /// <summary>
        /// Determines if two <see cref="TimeSignature"/> objects are equal.
        /// </summary>
        /// <param name="timeSignature1">The first <see cref="TimeSignature"/> to compare.</param>
        /// <param name="timeSignature2">The second <see cref="TimeSignature"/> to compare.</param>
        /// <returns>true if the time signatures are equal, false otherwise.</returns>
        public static bool operator ==(TimeSignature timeSignature1, TimeSignature timeSignature2)
        {
            if (ReferenceEquals(timeSignature1, timeSignature2))
                return true;

            if (ReferenceEquals(null, timeSignature1) || ReferenceEquals(null, timeSignature2))
                return false;

            return timeSignature1.Numerator == timeSignature2.Numerator &&
                   timeSignature1.Denominator == timeSignature2.Denominator;
        }

        /// <summary>
        /// Determines if two <see cref="TimeSignature"/> objects are not equal.
        /// </summary>
        /// <param name="timeSignature1">The first <see cref="TimeSignature"/> to compare.</param>
        /// <param name="timeSignature2">The second <see cref="TimeSignature"/> to compare.</param>
        /// <returns>false if the time signatures are equal, true otherwise.</returns>
        public static bool operator !=(TimeSignature timeSignature1, TimeSignature timeSignature2)
        {
            return !(timeSignature1 == timeSignature2);
        }

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
            return this == (obj as TimeSignature);
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
