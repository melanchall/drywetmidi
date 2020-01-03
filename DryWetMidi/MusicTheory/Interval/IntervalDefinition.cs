using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
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

        public IntervalDefinition(int number, IntervalQuality quality)
        {
            ThrowIfArgument.IsLessThan(nameof(number), number, 1, "Interval number is less than 1.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(quality), quality);

            Number = number;
            Quality = quality;
        }

        #endregion

        #region Properties

        public int Number { get; }

        public IntervalQuality Quality { get; }

        #endregion

        #region Operators

        public static bool operator ==(IntervalDefinition intervalDefinition1, IntervalDefinition intervalDefinition2)
        {
            if (ReferenceEquals(intervalDefinition1, intervalDefinition2))
                return true;

            if (ReferenceEquals(null, intervalDefinition1) || ReferenceEquals(null, intervalDefinition2))
                return false;

            return intervalDefinition1.Number == intervalDefinition2.Number &&
                   intervalDefinition1.Quality == intervalDefinition2.Quality;
        }

        public static bool operator !=(IntervalDefinition intervalDefinition1, IntervalDefinition intervalDefinition2)
        {
            return !(intervalDefinition1 == intervalDefinition2);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{QualitiesSymbols[Quality]}{Number}";
        }

        public override bool Equals(object obj)
        {
            return this == (obj as IntervalDefinition);
        }

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
