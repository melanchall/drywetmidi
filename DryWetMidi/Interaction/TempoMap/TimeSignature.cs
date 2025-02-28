using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents time signature which is number of beats of specified length.
    /// </summary>
    /// <seealso cref="TempoMap"/>
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="numerator"/> is zero or negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="denominator"/> is zero or negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="denominator"/> is not a power of two.</description>
        /// </item>
        /// </list>
        /// </exception>
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
        /// <returns><c>true</c> if the time signatures are equal, <c>false</c> otherwise.</returns>
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
        /// <returns><c>false</c> if the time signatures are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(TimeSignature timeSignature1, TimeSignature timeSignature2)
        {
            return !(timeSignature1 == timeSignature2);
        }

        /// <summary>
        /// Indicates whether a specified <see cref="TimeSignature"/> is less than another one.
        /// </summary>
        /// <param name="timeSignature1">The first <see cref="TimeSignature"/> to compare.</param>
        /// <param name="timeSignature2">The second <see cref="TimeSignature"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="timeSignature1"/> is less than the value of
        /// <paramref name="timeSignature2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeSignature1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSignature2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator <(TimeSignature timeSignature1, TimeSignature timeSignature2)
        {
            ThrowIfArgument.IsNull(nameof(timeSignature1), timeSignature1);
            ThrowIfArgument.IsNull(nameof(timeSignature2), timeSignature2);

            return timeSignature1.Numerator / (double)timeSignature1.Denominator < timeSignature2.Numerator / (double)timeSignature2.Denominator;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="TimeSignature"/> is less than or equal to
        /// another one.
        /// </summary>
        /// <param name="timeSignature1">The first <see cref="TimeSignature"/> to compare.</param>
        /// <param name="timeSignature2">The second <see cref="TimeSignature"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="timeSignature1"/> is less than or equal to the value of
        /// <paramref name="timeSignature2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeSignature1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSignature2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator <=(TimeSignature timeSignature1, TimeSignature timeSignature2)
        {
            ThrowIfArgument.IsNull(nameof(timeSignature1), timeSignature1);
            ThrowIfArgument.IsNull(nameof(timeSignature2), timeSignature2);

            return timeSignature1.Numerator / (double)timeSignature1.Denominator <= timeSignature2.Numerator / (double)timeSignature2.Denominator;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="TimeSignature"/> is greater than another one.
        /// </summary>
        /// <param name="timeSignature1">The first <see cref="TimeSignature"/> to compare.</param>
        /// <param name="timeSignature2">The second <see cref="TimeSignature"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="timeSignature1"/> is greater than the value of
        /// <paramref name="timeSignature2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeSignature1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSignature2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator >(TimeSignature timeSignature1, TimeSignature timeSignature2)
        {
            ThrowIfArgument.IsNull(nameof(timeSignature1), timeSignature1);
            ThrowIfArgument.IsNull(nameof(timeSignature2), timeSignature2);

            return timeSignature1.Numerator / (double)timeSignature1.Denominator > timeSignature2.Numerator / (double)timeSignature2.Denominator;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="TimeSignature"/> is greater than or equal to
        /// another one.
        /// </summary>
        /// <param name="timeSignature1">The first <see cref="TimeSignature"/> to compare.</param>
        /// <param name="timeSignature2">The second <see cref="TimeSignature"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="timeSignature1"/> is greater than or equal to the value of
        /// <paramref name="timeSignature2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeSignature1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSignature2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator >=(TimeSignature timeSignature1, TimeSignature timeSignature2)
        {
            ThrowIfArgument.IsNull(nameof(timeSignature1), timeSignature1);
            ThrowIfArgument.IsNull(nameof(timeSignature2), timeSignature2);

            return timeSignature1.Numerator / (double)timeSignature1.Denominator >= timeSignature2.Numerator / (double)timeSignature2.Denominator;
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
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
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
            unchecked
            {
                var result = 17;
                result = result * 23 + Numerator.GetHashCode();
                result = result * 23 + Denominator.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
