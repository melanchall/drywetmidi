using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class IntervalParser : SimpleParser<Interval>
    {
        #region Constants

        private static readonly Dictionary<char, IntervalQuality> IntervalQualitiesByLetters =
            new Dictionary<char, IntervalQuality>
            {
                ['P'] = IntervalQuality.Perfect,
                ['p'] = IntervalQuality.Perfect,
                ['M'] = IntervalQuality.Major,
                ['m'] = IntervalQuality.Minor,
                ['D'] = IntervalQuality.Diminished,
                ['d'] = IntervalQuality.Diminished,
                ['A'] = IntervalQuality.Augmented,
                ['a'] = IntervalQuality.Augmented
            };

        private const string HalfStepsNumberIsOutOfRange = "Interval's half steps number is out of range.";
        private const string IntervalNumberIsOutOfRange = "Interval's number is out of range.";

        #endregion

        #region Methods

        public (Interval? Interval, int Length) TryReadInterval(ReadOnlySpan<char> input)
        {
            var endIndex = 1;

            if (input[0] == '+' || input[0] == '-' || char.IsDigit(input[0]))
            {
                for (; endIndex < input.Length; endIndex++)
                {
                    if (!char.IsDigit(input[endIndex]))
                        break;
                }

                if (!int.TryParse(input.Slice(0, endIndex), out var halfSteps))
                    return (null, 0);

                if (!IntervalUtilities.IsIntervalValid(halfSteps))
                    return (null, 0);

                return (Interval.FromHalfSteps(halfSteps), endIndex);
            }

            if (!IntervalQualitiesByLetters.TryGetValue(input[0], out var intervalQuality))
                return (null, 0);

            for (; endIndex < input.Length; endIndex++)
            {
                if (!char.IsDigit(input[endIndex]))
                    break;
            }

            if (!int.TryParse(input.Slice(1, endIndex - 1), out var intervalNumber))
                return (null, 0);

            return (Interval.Get(intervalQuality, intervalNumber), endIndex);
        }

        internal override Regex[] GetRegexes()
        {
            throw new NotImplementedException();
        }

        protected override Interval ParseInternal(string input)
        {
            if (input[0] == '+' || input[0] == '-' || char.IsDigit(input[0]))
            {
                if (!int.TryParse(input, out var halfSteps))
                    ThrowInvalidFormatError();

                if (!IntervalUtilities.IsIntervalValid(halfSteps))
                    ThrowError(HalfStepsNumberIsOutOfRange);

                return Interval.FromHalfSteps(halfSteps);
            }

            var intervalQualityLetter = input[0];
            if (!IntervalQualitiesByLetters.TryGetValue(intervalQualityLetter, out var intervalQuality))
                ThrowInvalidFormatError();

            if (!int.TryParse(input.Substring(1), out var intervalNumber))
                ThrowError(IntervalNumberIsOutOfRange);

            return Interval.Get(intervalQuality, intervalNumber);
        }

        #endregion
    }
}
