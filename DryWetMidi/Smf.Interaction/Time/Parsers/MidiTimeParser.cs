using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MidiTimeParser
    {
        #region Nested types

        internal enum ParsingResult
        {
            Parsed,

            InputStringIsNullOrWhiteSpace,
            NotMatched,
            OutOfRange,
        }

        #endregion

        #region Constants

        private const string TimeGroupName = "t";

        private static readonly string TimeGroup = ParsingUtilities.GetNumberGroup(TimeGroupName);

        private static readonly string[] Patterns = new[]
        {
            $@"{TimeGroup}",
        };

        private static readonly Dictionary<ParsingResult, string> FormatExceptionMessages =
            new Dictionary<ParsingResult, string>
            {
                [ParsingResult.NotMatched] = "Input string has invalid musical time format.",
                [ParsingResult.OutOfRange] = "Time is out of range.",
            };

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out MidiTime time)
        {
            time = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.InputStringIsNullOrWhiteSpace;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            if (!ParsingUtilities.ParseLong(match, TimeGroupName, 0, out var midiTime))
                return ParsingResult.OutOfRange;

            time = new MidiTime(midiTime);
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
