using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class BarBeatCentsTimeSpanParser
    {
        #region Constants

        private const string BarsGroupName = "bars";
        private const string BeatsGroupName = "beats";
        private const string CentsGroupName = "cents";

        private static readonly string BarsGroup = ParsingUtilities.GetNonnegativeIntegerNumberGroup(BarsGroupName);
        private static readonly string BeatsGroup = ParsingUtilities.GetNonnegativeIntegerNumberGroup(BeatsGroupName);
        private static readonly string CentsGroup = ParsingUtilities.GetNonnegativeDoubleNumberGroup(CentsGroupName);

        private static readonly string Divider = Regex.Escape(".");

        private static readonly string[] Patterns = new[]
        {
            $@"{BarsGroup}\s*{Divider}\s*{BeatsGroup}\s*{Divider}\s*{CentsGroup}",
        };

        private const string BarsIsOutOfRange = "Bars number is out of range.";
        private const string BeatsIsOutOfRange = "Beats number is out of range.";
        private const string CentsIsOutOfRange = "Cents number is out of range.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out BarBeatCentsTimeSpan timeSpan)
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

            long beats;
            if (!ParsingUtilities.ParseNonnegativeLong(match, BeatsGroupName, 0, out beats))
                return ParsingResult.Error(BeatsIsOutOfRange);

            double cents;
            if (!ParsingUtilities.ParseNonnegativeDouble(match, CentsGroupName, 0, out cents) ||
                cents > BarBeatCentsTimeSpan.MaxCents)
                return ParsingResult.Error(CentsIsOutOfRange);

            timeSpan = new BarBeatCentsTimeSpan(bars, beats, cents);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
