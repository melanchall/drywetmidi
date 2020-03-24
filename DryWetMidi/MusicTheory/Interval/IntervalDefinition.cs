using System;
using System.Collections.Generic;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Definition of a musical interval which is number and quality.
    /// </summary>
    public sealed class IntervalDefinition
    {
        #region Constants

        private static readonly Dictionary<IntervalQuality, char> QualitiesSymbols = new Dictionary<IntervalQuality, char>
        {
            [IntervalQuality.Perfect] = 'P',
            [IntervalQuality.Minor] = 'm',
            [IntervalQuality.Major] = 'M',
            [IntervalQuality.Augmented] = 'A',
            [IntervalQuality.Diminished] = 'd'
        };

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalDefinition"/> with the specified
        /// interval number and quality.
        /// </summary>
        /// <param name="number">Interval number.</param>
        /// <param name="quality">Interval quality.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is less than 1.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="quality"/> specified an invalid value.</exception>
        public IntervalDefinition(int number, IntervalQuality quality)
        {
            ThrowIfArgument.IsLessThan(nameof(number), number, 1, "Interval number is less than 1.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(quality), quality);

            Number = number;
            Quality = quality;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the interval number.
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Gets the interval quality.
        /// </summary>
        public IntervalQuality Quality { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="IntervalDefinition"/> objects are equal.
        /// </summary>
        /// <param name="intervalDefinition1">The first <see cref="IntervalDefinition"/> to compare.</param>
        /// <param name="intervalDefinition2">The second <see cref="IntervalDefinition"/> to compare.</param>
        /// <returns><c>true</c> if the interval definitions are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(IntervalDefinition intervalDefinition1, IntervalDefinition intervalDefinition2)
        {
            if (ReferenceEquals(intervalDefinition1, intervalDefinition2))
                return true;

            if (ReferenceEquals(null, intervalDefinition1) || ReferenceEquals(null, intervalDefinition2))
                return false;

            return intervalDefinition1.Number == intervalDefinition2.Number &&
                   intervalDefinition1.Quality == intervalDefinition2.Quality;
        }

        /// <summary>
        /// Determines if two <see cref="IntervalDefinition"/> objects are not equal.
        /// </summary>
        /// <param name="intervalDefinition1">The first <see cref="IntervalDefinition"/> to compare.</param>
        /// <param name="intervalDefinition2">The second <see cref="IntervalDefinition"/> to compare.</param>
        /// <returns><c>false</c> if the interval definitions are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(IntervalDefinition intervalDefinition1, IntervalDefinition intervalDefinition2)
        {
            return !(intervalDefinition1 == intervalDefinition2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{QualitiesSymbols[Quality]}{Number}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as IntervalDefinition);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + Number.GetHashCode();
                result = result * 23 + Quality.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
