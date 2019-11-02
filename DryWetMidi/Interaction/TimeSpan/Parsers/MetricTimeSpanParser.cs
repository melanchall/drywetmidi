using Melanchall.DryWetMidi.Common;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class MetricTimeSpanParser
    {
        #region Constants

        private const string HoursGroupName = "h";
        private const string MinutesGroupName = "m";
        private const string SecondsGroupName = "s";
        private const string MillisecondsGroupName = "ms";

        private static readonly string HoursGroup = ParsingUtilities.GetNonnegativeIntegerNumberGroup(HoursGroupName);
        private static readonly string MinutesGroup = ParsingUtilities.GetNonnegativeIntegerNumberGroup(MinutesGroupName);
        private static readonly string SecondsGroup = ParsingUtilities.GetNonnegativeIntegerNumberGroup(SecondsGroupName);
        private static readonly string MillisecondsGroup = ParsingUtilities.GetNonnegativeIntegerNumberGroup(MillisecondsGroupName);

        private static readonly string LetteredHoursGroup = $@"{HoursGroup}\s*h";
        private static readonly string LetteredMinutesGroup = $@"{MinutesGroup}\s*m";
        private static readonly string LetteredSecondsGroup = $@"{SecondsGroup}\s*s";
        private static readonly string LetteredMillisecondsGroup = $@"{MillisecondsGroup}\s*ms";

        private static readonly string Divider = Regex.Escape(":");

        private static readonly string[] Patterns = new[]
        {
            // hours:minutes:seconds:milliseconds -> hours:minutes:seconds:milliseconds
            $@"{HoursGroup}\s*{Divider}\s*{MinutesGroup}\s*{Divider}\s*{SecondsGroup}\s*{Divider}\s*{MillisecondsGroup}",

            // hours:minutes:seconds -> hours:minutes:seconds:0
            $@"{HoursGroup}\s*{Divider}\s*{MinutesGroup}\s*{Divider}\s*{SecondsGroup}",

            // minutes:seconds -> 0:minutes:seconds:0
            $@"{MinutesGroup}\s*{Divider}\s*{SecondsGroup}",

            // hours h minutes m seconds s milliseconds ms -> hours:minutes:seconds:milliseconds
            $@"{LetteredHoursGroup}\s*{LetteredMinutesGroup}\s*{LetteredSecondsGroup}\s*{LetteredMillisecondsGroup}",

            // hours h minutes m seconds s -> hours:minutes:seconds:0
            $@"{LetteredHoursGroup}\s*{LetteredMinutesGroup}\s*{LetteredSecondsGroup}",

            // hours h minutes m milliseconds ms -> hours:minutes:0:milliseconds
            $@"{LetteredHoursGroup}\s*{LetteredMinutesGroup}\s*{LetteredMillisecondsGroup}",

            // hours h seconds s milliseconds ms -> hours:0:seconds:milliseconds
            $@"{LetteredHoursGroup}\s*{LetteredSecondsGroup}\s*{LetteredMillisecondsGroup}",

            // minutes m seconds s milliseconds ms -> 0:minutes:seconds:milliseconds
            $@"{LetteredMinutesGroup}\s*{LetteredSecondsGroup}\s*{LetteredMillisecondsGroup}",

            // hours h minutes m -> hours:minutes:0:0
            $@"{LetteredHoursGroup}\s*{LetteredMinutesGroup}",

            // hours h seconds s -> hours:0:seconds:0
            $@"{LetteredHoursGroup}\s*{LetteredSecondsGroup}",

            // hours h milliseconds ms -> hours:0:0:milliseconds
            $@"{LetteredHoursGroup}\s*{LetteredMillisecondsGroup}",

            // minutes m seconds s -> 0:minutes:seconds:0
            $@"{LetteredMinutesGroup}\s*{LetteredSecondsGroup}",

            // hours h milliseconds ms -> hours:0:0:milliseconds
            $@"{LetteredMinutesGroup}\s*{LetteredMillisecondsGroup}",

            // seconds s milliseconds ms -> 0:0:seconds:milliseconds
            $@"{LetteredSecondsGroup}\s*{LetteredMillisecondsGroup}",

            // hours h -> hours:0:0:0
            LetteredHoursGroup,

            // minutes m -> 0:minutes:0:0
            LetteredMinutesGroup,

            // seconds s -> 0:0:seconds:0
            LetteredSecondsGroup,

            // milliseconds ms -> 0:0:0:milliseconds
            LetteredMillisecondsGroup
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
