using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MathLengthParser
    {
        #region Constants

        private const string Length1GroupName = "l1";
        private const string Length2GroupName = "l2";
        private const string OperationGroupName = "op";

        private static readonly string Length1Group = $@"(?<{Length1GroupName}>.+?)";
        private static readonly string Length2Group = $@"(?<{Length2GroupName}>.+?)";

        private static readonly string[] Patterns = new[]
        {
            // time +/- offset
            $@"\(?{Length1Group}\s*(?<{OperationGroupName}>[\+\-])\s*{Length2Group}\)?",
        };

        private const string InvalidLength1Format = "First length has invalid format.";
        private const string InvalidLength2Format = "Second length has invalid format.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out MathLength length)
        {
            length = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            var length1Group = match.Groups[Length1GroupName];
            if (!LengthUtilities.TryParse(length1Group.Value, out var length1))
                return new ParsingResult(InvalidLength1Format);

            var length2Group = match.Groups[Length2GroupName];
            if (!LengthUtilities.TryParse(length2Group.Value, out var length2))
                return new ParsingResult(InvalidLength2Format);

            var operationGroup = match.Groups[OperationGroupName];
            var operation = operationGroup.Value == "+"
                ? MathOperation.Add
                : MathOperation.Subtract;

            length = new MathLength(length1, length2, operation);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
