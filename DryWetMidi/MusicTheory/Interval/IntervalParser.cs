using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class IntervalParser
    {
        #region Constants

        private const string HalfStepsGroupName = "hs";

        private static readonly string HalfStepsGroup = ParsingUtilities.GetNumberGroup(HalfStepsGroupName);

        private static readonly string[] Patterns = new[]
        {
            $@"{HalfStepsGroup}"
        };

        private const string HalfStepsNumberIsOutOfRange = "Interval's half steps number is out of range.";

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

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            int halfSteps;
            if (!ParsingUtilities.ParseInt(match, HalfStepsGroupName, 0, out halfSteps) ||
                !IntervalUtilities.IsIntervalValid(halfSteps))
                return ParsingResult.Error(HalfStepsNumberIsOutOfRange);

            interval = Interval.FromHalfSteps(halfSteps);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
