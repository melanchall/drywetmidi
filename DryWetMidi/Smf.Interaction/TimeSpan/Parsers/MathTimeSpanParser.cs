using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MathTimeSpanParser
    {
        #region Constants

        private const string TimeSpan1GroupName = "ts1";
        private const string TimeSpan2GroupName = "ts2";
        private const string OperationGroupName = "op";

        private static readonly string TimeSpan1Group = $@"(?<{TimeSpan1GroupName}>.+?)";
        private static readonly string TimeSpan2Group = $@"(?<{TimeSpan2GroupName}>.+?)";

        private static readonly string[] Patterns = new[]
        {
            $@"\(?{TimeSpan1Group}\s*(?<{OperationGroupName}>[\+\-])\s*{TimeSpan2Group}\)?",
        };

        private const string InvalidLength1Format = "First time span has invalid format.";
        private const string InvalidLength2Format = "Second time span has invalid format.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out MathTimeSpan timeSpan)
        {
            timeSpan = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            var timeSpan1Group = match.Groups[TimeSpan1GroupName];
            if (!TimeSpanUtilities.TryParse(timeSpan1Group.Value, out var timeSpan1))
                return new ParsingResult(InvalidLength1Format);

            var timeSpan2Group = match.Groups[TimeSpan2GroupName];
            if (!TimeSpanUtilities.TryParse(timeSpan2Group.Value, out var timeSpan2))
                return new ParsingResult(InvalidLength2Format);

            var operationGroup = match.Groups[OperationGroupName];
            var operation = operationGroup.Value == "+"
                ? MathOperation.Add
                : MathOperation.Subtract;

            timeSpan = new MathTimeSpan(timeSpan1, timeSpan2, operation);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
