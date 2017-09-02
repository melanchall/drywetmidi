using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MathTimeParser
    {
        #region Constants

        private const string TimeGroupName = "t";
        private const string OffsetGroupName = "o";
        private const string OperationGroupName = "op";

        private static readonly string TimeGroup = $@"(?<{TimeGroupName}>.+?)";
        private static readonly string OffsetGroup = $@"(?<{OffsetGroupName}>.+)";

        private static readonly string[] Patterns = new[]
        {
            // time +/- offset
            $@"\(?{TimeGroup}\s*(?<{OperationGroupName}>[\+\-])\s*{OffsetGroup}\)?",
        };

        private const string InvalidTimeFormat = "Time has invalid format.";
        private const string InvalidOffsetFormat = "Offset has invalid format.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out MathTime time)
        {
            time = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            var timeGroup = match.Groups[TimeGroupName];
            if (!TimeUtilities.TryParse(timeGroup.Value, out var baseTime))
                return new ParsingResult(InvalidTimeFormat);

            var offsetGroup = match.Groups[OffsetGroupName];
            if (!LengthUtilities.TryParse(offsetGroup.Value, out var offset))
                return new ParsingResult(InvalidOffsetFormat);

            var operationGroup = match.Groups[OperationGroupName];
            var operation = operationGroup.Value == "+"
                ? MathOperation.Add
                : MathOperation.Subtract;

            time = new MathTime(baseTime, offset, operation);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
