using Melanchall.DryWetMidi.Common;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class BarBeatTicksTimeSpanParser
    {
        #region Constants

        private const string BarsGroupName = "bars";
        private const string BeatsGroupName = "beats";
        private const string TicksGroupName = "ticks";

        private static readonly string BarsGroup = ParsingUtilities.GetNonnegativeIntegerNumberGroup(BarsGroupName);
        private static readonly string BeatsGroup = ParsingUtilities.GetNonnegativeIntegerNumberGroup(BeatsGroupName);
        private static readonly string TicksGroup = ParsingUtilities.GetNonnegativeIntegerNumberGroup(TicksGroupName);

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

        internal static ParsingResult TryParse(string input, out BarBeatTicksTimeSpan timeSpan)
        {
            timeSpan = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            if (!ParsingUtilities.ParseNonnegativeLong(match, BarsGroupName, 0, out var bars))
                return ParsingResult.Error(BarsIsOutOfRange);

            if (!ParsingUtilities.ParseNonnegativeLong(match, BeatsGroupName, 0, out var beats))
                return ParsingResult.Error(BeatsIsOutOfRange);

            if (!ParsingUtilities.ParseNonnegativeLong(match, TicksGroupName, 0, out var ticks))
                return ParsingResult.Error(TicksIsOutOfRange);

            timeSpan = new BarBeatTicksTimeSpan(bars, beats, ticks);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
