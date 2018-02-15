using Melanchall.DryWetMidi.Common;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MetricTimeSpanParser
    {
        #region Constants

        private const string HoursGroupName = "h";
        private const string MinutesGroupName = "m";
        private const string SecondsGroupName = "s";
        private const string MillisecondsGroupName = "ms";

        private static readonly string HoursGroup = ParsingUtilities.GetNonnegativeNumberGroup(HoursGroupName);
        private static readonly string MinutesGroup = ParsingUtilities.GetNonnegativeNumberGroup(MinutesGroupName);
        private static readonly string SecondsGroup = ParsingUtilities.GetNonnegativeNumberGroup(SecondsGroupName);
        private static readonly string MillisecondsGroup = ParsingUtilities.GetNonnegativeNumberGroup(MillisecondsGroupName);

        private static readonly string Divider = Regex.Escape(":");

        private static readonly string[] Patterns = new[]
        {
            // hours:minutes:seconds:milliseconds -> hours:minutes:seconds:milliseconds
            $@"{HoursGroup}\s*{Divider}\s*{MinutesGroup}\s*{Divider}\s*{SecondsGroup}\s*{Divider}\s*{MillisecondsGroup}",

            // hours:minutes:seconds -> hours:minutes:seconds:0
            $@"{HoursGroup}\s*{Divider}\s*{MinutesGroup}\s*{Divider}\s*{SecondsGroup}",

            // minutes:seconds -> 0:minutes:seconds:0
            $@"{MinutesGroup}\s*{Divider}\s*{SecondsGroup}",
        };

        private const string HoursIsOutOfRange = "Hours number is out of range.";
        private const string MinutesIsOutOfRange = "Minutes number is out of range.";
        private const string SecondsIsOutOfRange = "Seconds number is out of range.";
        private const string MillisecondsIsOutOfRange = "Milliseconds number is out of range.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out MetricTimeSpan timeSpan)
        {
            timeSpan = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            int hours;
            if (!ParsingUtilities.ParseNonnegativeInt(match, HoursGroupName, 0, out hours))
                return ParsingResult.Error(HoursIsOutOfRange);

            int minutes;
            if (!ParsingUtilities.ParseNonnegativeInt(match, MinutesGroupName, 0, out minutes))
                return ParsingResult.Error(MinutesIsOutOfRange);

            int seconds;
            if (!ParsingUtilities.ParseNonnegativeInt(match, SecondsGroupName, 0, out seconds))
                return ParsingResult.Error(SecondsIsOutOfRange);

            int milliseconds;
            if (!ParsingUtilities.ParseNonnegativeInt(match, MillisecondsGroupName, 0, out milliseconds))
                return ParsingResult.Error(MillisecondsIsOutOfRange);

            timeSpan = new MetricTimeSpan(hours, minutes, seconds, milliseconds);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
