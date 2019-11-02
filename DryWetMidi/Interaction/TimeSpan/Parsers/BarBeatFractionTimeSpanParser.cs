using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class BarBeatFractionTimeSpanParser
    {
        #region Constants

        private const string BarsGroupName = "bars";
        private const string BeatsGroupName = "beats";

        private static readonly string BarsGroup = ParsingUtilities.GetNonnegativeIntegerNumberGroup(BarsGroupName);
        private static readonly string BeatsGroup = ParsingUtilities.GetNonnegativeDoubleNumberGroup(BeatsGroupName);

        private static readonly string Divider = Regex.Escape("_");

        private static readonly string[] Patterns = new[]
        {
            $@"{BarsGroup}\s*{Divider}\s*{BeatsGroup}",
        };

        private const string BarsIsOutOfRange = "Bars number is out of range.";
        private const string BeatsIsOutOfRange = "Beats number is out of range.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out BarBeatFractionTimeSpan timeSpan)
        {
            timeSpan = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            long bars;
            if (!ParsingUtilities.ParseNonnegativeLong(match, BarsGroupName, 0, out bars))
                return ParsingResult.Error(BarsIsOutOfRange);

            double beats;
            if (!ParsingUtilities.ParseNonnegativeDouble(match, BeatsGroupName, 0, out beats))
                return ParsingResult.Error(BeatsIsOutOfRange);

            timeSpan = new BarBeatFractionTimeSpan(bars, beats);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
