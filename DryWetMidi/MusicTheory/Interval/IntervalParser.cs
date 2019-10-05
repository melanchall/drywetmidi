using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class IntervalParser
    {
        #region Constants

        private const string HalfStepsGroupName = "hs";
        private const string IntervalQualityGroupName = "q";
        private const string IntervalNumberGroupName = "n";

        private static readonly string HalfStepsGroup = ParsingUtilities.GetIntegerNumberGroup(HalfStepsGroupName);
        private static readonly string IntervalGroup = $@"(?<{IntervalQualityGroupName}>P|p|M|m|D|d|A|a)(?<{IntervalNumberGroupName}>\d+)";

        private static readonly string[] Patterns = new[]
        {
            IntervalGroup,
            HalfStepsGroup
        };

        private static readonly Dictionary<string, IntervalQuality> IntervalQualitiesByLetters =
            new Dictionary<string, IntervalQuality>
            {
                ["P"] = IntervalQuality.Perfect,
                ["p"] = IntervalQuality.Perfect,
                ["M"] = IntervalQuality.Major,
                ["m"] = IntervalQuality.Minor,
                ["D"] = IntervalQuality.Diminished,
                ["d"] = IntervalQuality.Diminished,
                ["A"] = IntervalQuality.Augmented,
                ["a"] = IntervalQuality.Augmented
            };

        private const string HalfStepsNumberIsOutOfRange = "Interval's half steps number is out of range.";
        private const string IntervalNumberIsOutOfRange = "Interval's number is out of range.";

        #endregion

        #region Methods

        internal static IEnumerable<string> GetPatterns()
        {
            return Patterns;
        }

        internal static ParsingResult TryParse(string input, out Interval interval)
        {
            interval = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns, ignoreCase: false);
            if (match == null)
                return ParsingResult.NotMatched;

            var intervalQualityGroup = match.Groups[IntervalQualityGroupName];
            if (!intervalQualityGroup.Success)
            {
                int halfSteps;
                if (!ParsingUtilities.ParseInt(match, HalfStepsGroupName, 0, out halfSteps) ||
                    !IntervalUtilities.IsIntervalValid(halfSteps))
                    return ParsingResult.Error(HalfStepsNumberIsOutOfRange);

                interval = Interval.FromHalfSteps(halfSteps);
                return ParsingResult.Parsed;
            }

            var intervalQuality = IntervalQualitiesByLetters[intervalQualityGroup.Value];

            int intervalNumber;
            if (!ParsingUtilities.ParseInt(match, IntervalNumberGroupName, 0, out intervalNumber) || intervalNumber < 1)
                return ParsingResult.Error(IntervalNumberIsOutOfRange);

            interval = Interval.Get(intervalQuality, intervalNumber);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
