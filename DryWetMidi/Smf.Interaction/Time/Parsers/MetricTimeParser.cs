using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MetricTimeParser
    {
        #region Nested types

        internal enum ParsingResult
        {
            Parsed,

            InputStringIsNullOrWhiteSpace,
            NotMatched,
            HoursIsOutOfRange,
            MinutesIsOutOfRange,
            SecondsIsOutOfRange,
            MillisecondsIsOutOfRange,
        }

        #endregion

        #region Constants

        private const string HoursGroupName = "h";
        private const string MinutesGroupName = "m";
        private const string SecondsGroupName = "s";
        private const string MillisecondsGroupName = "ms";

        private static readonly string HoursGroup = ParsingUtilities.GetNumberGroup(HoursGroupName);
        private static readonly string MinutesGroup = ParsingUtilities.GetNumberGroup(MinutesGroupName);
        private static readonly string SecondsGroup = ParsingUtilities.GetNumberGroup(SecondsGroupName);
        private static readonly string MillisecondsGroup = ParsingUtilities.GetNumberGroup(MillisecondsGroupName);

        private static readonly string Divider = Regex.Escape(":");

        private static readonly string[] Patterns = new[]
        {
            // hours:minutes:seconds:milliseconds -> hours:minutes:seconds:milliseconds
            $@"{HoursGroup}\s*{Divider}\s*{MinutesGroup}\s*{Divider}\s*{SecondsGroup}\s*{Divider}\s*{MillisecondsGroup}",

            // hours:minutes:seconds -> hours:minutes:seconds:0
            $@"{HoursGroup}\s*{Divider}\s*{MinutesGroup}\s*{Divider}\s*{SecondsGroup}",

            // minutes:seconds -> 0:minutes:seconds:0
            $@"{MinutesGroup}\s*{Divider}\s*{SecondsGroup}",

            // milliseconds -> 0:0:0:milliseconds
            $@"{MillisecondsGroup}",
        };

        private static readonly Dictionary<ParsingResult, string> FormatExceptionMessages =
            new Dictionary<ParsingResult, string>
            {
                [ParsingResult.NotMatched] = "Input string has invalid musical time format.",
                [ParsingResult.HoursIsOutOfRange] = "Hours number is out of range.",
                [ParsingResult.MinutesIsOutOfRange] = "Minutes number is out of range.",
                [ParsingResult.SecondsIsOutOfRange] = "Seconds number is out of range.",
                [ParsingResult.MillisecondsIsOutOfRange] = "Milliseconds number is out of range."
            };

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out MetricTime time)
        {
            time = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.InputStringIsNullOrWhiteSpace;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            if (!ParsingUtilities.ParseInt(match, HoursGroupName, 0, out var hours))
                return ParsingResult.HoursIsOutOfRange;

            if (!ParsingUtilities.ParseInt(match, MinutesGroupName, 0, out var minutes))
                return ParsingResult.MinutesIsOutOfRange;

            if (!ParsingUtilities.ParseInt(match, SecondsGroupName, 0, out var seconds))
                return ParsingResult.SecondsIsOutOfRange;

            if (!ParsingUtilities.ParseInt(match, MillisecondsGroupName, 0, out var milliseconds))
                return ParsingResult.MillisecondsIsOutOfRange;

            time = new MetricTime(hours, minutes, seconds, milliseconds);
            return ParsingResult.Parsed;
        }

        internal static Exception GetException(ParsingResult parsingResult, string inputStringParameterName)
        {
            if (parsingResult == ParsingResult.InputStringIsNullOrWhiteSpace)
                return new ArgumentException("Input string is null or contains white-spaces only.", inputStringParameterName);

            return FormatExceptionMessages.TryGetValue(parsingResult, out var formatExceptionMessage)
                ? new FormatException(formatExceptionMessage)
                : null;
        }

        #endregion
    }
}
