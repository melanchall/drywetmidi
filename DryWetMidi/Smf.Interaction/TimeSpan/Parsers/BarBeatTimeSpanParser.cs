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

        private static readonly string BarsGroup = ParsingUtilities.GetNumberGroup(BarsGroupName);
        private static readonly string BeatsGroup = ParsingUtilities.GetNumberGroup(BeatsGroupName);
        private static readonly string TicksGroup = ParsingUtilities.GetNumberGroup(TicksGroupName);

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

            if (!ParsingUtilities.ParseLong(match, BarsGroupName, 0, out var bars))
                return new ParsingResult(BarsIsOutOfRange);

            if (!ParsingUtilities.ParseLong(match, BeatsGroupName, 0, out var beats))
                return new ParsingResult(BeatsIsOutOfRange);

            if (!ParsingUtilities.ParseLong(match, TicksGroupName, 0, out var ticks))
                return new ParsingResult(BeatsIsOutOfRange);

            timeSpan = new BarBeatTimeSpan(bars, beats, ticks);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
