using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class IntervalParser : SimpleParser<Interval>
    {
        #region Constants

        private const string HalfStepsGroupName = "hs";
        private const string IntervalQualityGroupName = "q";
        private const string IntervalNumberGroupName = "n";

        private static readonly string HalfStepsGroup = GetIntegerNumberGroup(HalfStepsGroupName);
        private static readonly string IntervalGroup = $@"(?<{IntervalQualityGroupName}>P|p|M|m|D|d|A|a)(?<{IntervalNumberGroupName}>\d+)";

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

        internal override IEnumerable<string> GetPatterns() => new[]
        {
            IntervalGroup,
            HalfStepsGroup,
        };

        protected override Interval ParseInternal(string input)
        {
            var match = Match(input, ignoreCase: false);
            if (match == null)
                ThrowInvalidFormatError();

            var intervalQualityGroup = match.Groups[IntervalQualityGroupName];
            if (!intervalQualityGroup.Success)
            {
                if (!ParseInt(match, HalfStepsGroupName, 0, out var halfSteps) ||
                    !IntervalUtilities.IsIntervalValid(halfSteps))
                    ThrowError(HalfStepsNumberIsOutOfRange);

                return Interval.FromHalfSteps(halfSteps);
            }

            var intervalQuality = IntervalQualitiesByLetters[intervalQualityGroup.Value];

            if (!ParseInt(match, IntervalNumberGroupName, 0, out var intervalNumber) || intervalNumber < 1)
                ThrowError(IntervalNumberIsOutOfRange);

            return Interval.Get(intervalQuality, intervalNumber);
        }

        #endregion
    }
}
