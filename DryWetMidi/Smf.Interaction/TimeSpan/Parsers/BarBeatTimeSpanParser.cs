using Melanchall.DryWetMidi.Common;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class BarBeatTimeSpanParser
    {
        #region Constants

        private const string BarsGroupName = "bars";
        private const string BeatsGroupName = "beats";
        private const string TicksGroupName = "ticks";

        private static readonly string BarsGroup = ParsingUtilities.GetNonnegativeNumberGroup(BarsGroupName);
        private static readonly string BeatsGroup = ParsingUtilities.GetNonnegativeNumberGroup(BeatsGroupName);
        private static readonly string TicksGroup = ParsingUtilities.GetNonnegativeNumberGroup(TicksGroupName);

        private static readonly string Divider = Regex.Escape(".");

        private static readonly string[] Patterns = new[]
        {
            $@"{BarsGroup}\s*{Divider}\s*{BeatsGroup}\s*{Divider}\s*{TicksGroup}",
        };

        private const string BarsIsOutOfRange = "Bars number is out of range.";
        private const string BeatsIsOutOfRange = "Beats number is out of range.";
        private const string TicksIsOutOfRange = "Ticks number is out of range.";

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

            long bars;
            if (!ParsingUtilities.ParseNonnegativeLong(match, BarsGroupName, 0, out bars))
                return ParsingResult.Error(BarsIsOutOfRange);

            long beats;
            if (!ParsingUtilities.ParseNonnegativeLong(match, BeatsGroupName, 0, out beats))
                return ParsingResult.Error(BeatsIsOutOfRange);

            long ticks;
            if (!ParsingUtilities.ParseNonnegativeLong(match, TicksGroupName, 0, out ticks))
                return ParsingResult.Error(TicksIsOutOfRange);

            timeSpan = new BarBeatTimeSpan(bars, beats, ticks);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
