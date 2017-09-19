using Melanchall.DryWetMidi.Common;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class BarBeatTimeSpanParser
    {
        #region Constants

        private const string BarsGroupName = "bars";
        private const string BeatsGroupName = "beats";

        private static readonly string BarsGroup = $@"(?<{BarsGroupName}>\d+)";
        private static readonly string BeatsGroup = $@"(?<{BeatsGroupName}>\-?\d+)";

        private static readonly string Divider = Regex.Escape(".");

        private static readonly string[] Patterns = new[]
        {
            $@"{BarsGroup}\s*{Divider}\s*{BeatsGroup}",
        };

        private const string BarsIsOutOfRange = "Bars number is out of range.";
        private const string BeatsIsOutOfRange = "Beats number is out of range.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out BarBeatTimeSpan timeSpan)
        {
            timeSpan = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            if (!ParsingUtilities.ParseInt(match, BarsGroupName, 0, out var bars))
                return new ParsingResult(BarsIsOutOfRange);

            if (!ParsingUtilities.ParseInt(match, BeatsGroupName, 0, out var beats))
                return new ParsingResult(BeatsIsOutOfRange);

            timeSpan = new BarBeatTimeSpan(bars, beats);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
