using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MusicalTimeParser
    {
        #region Nested types

        internal enum ParsingResult
        {
            Parsed,

            InputStringIsNullOrWhiteSpace,
            NotMatched,
            BarsIsOutOfRange,
            BeatsIsOutOfRange,
            FractionNotMatched,
            FractionNumeratorIsOutOfRange,
            FractionDenominatorIsOutOfRange,
        }

        #endregion

        #region Constants

        private const string BarsGroupName = "B";
        private const string BeatsGroupName = "b";
        private const string FractionGroupName = "f";

        private static readonly string BarsGroup = ParsingUtilities.GetNumberGroup(BarsGroupName);
        private static readonly string BeatsGroup = ParsingUtilities.GetNumberGroup(BeatsGroupName);
        private static readonly string FractionGroup = $@"(?<{FractionGroupName}>.+)";

        private static readonly string Divider = Regex.Escape(".");

        private static readonly string[] Patterns = new[]
        {
            // bars.beats.fraction -> bars.beats.fraction
            $@"{BarsGroup}\s*{Divider}\s*{BeatsGroup}\s*{Divider}\s*{FractionGroup}",

            // bars.beats -> bars.beats.0/1
            $@"{BarsGroup}\s*{Divider}\s*{BeatsGroup}",

            // fraction -> 0.0.fraction
            $@"{FractionGroup}",
        };

        private static readonly Dictionary<ParsingResult, string> FormatExceptionMessages =
            new Dictionary<ParsingResult, string>
            {
                [ParsingResult.NotMatched] = "Input string has invalid musical time format.",
                [ParsingResult.BarsIsOutOfRange] = "Bars number is out of range.",
                [ParsingResult.BeatsIsOutOfRange] = "Beats number is out of range.",
                [ParsingResult.FractionNotMatched] = "Input string has invalid fraction format.",
                [ParsingResult.FractionNumeratorIsOutOfRange] = "Fraction's numerator is out of range.",
                [ParsingResult.FractionDenominatorIsOutOfRange] = "Fraction's denominator is out of range."
            };

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out MusicalTime time)
        {
            time = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.InputStringIsNullOrWhiteSpace;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            // Parse bars, beats

            if (!ParsingUtilities.ParseInt(match, BarsGroupName, 0, out var bars))
                return ParsingResult.BarsIsOutOfRange;

            if (!ParsingUtilities.ParseInt(match, BeatsGroupName, 0, out var beats))
                return ParsingResult.BeatsIsOutOfRange;

            // Parse fraction

            var fraction = Fraction.ZeroFraction;
            var fractionGroup = match.Groups[FractionGroupName];
            if (fractionGroup.Success)
            {
                switch (FractionParser.TryParse(fractionGroup.Value, out fraction))
                {
                    case FractionParser.ParsingResult.NotMatched:
                        return ParsingResult.FractionNotMatched;

                    case FractionParser.ParsingResult.NumeratorIsOutOfRange:
                        return ParsingResult.FractionNumeratorIsOutOfRange;

                    case FractionParser.ParsingResult.DenominatorIsOutOfRange:
                        return ParsingResult.FractionDenominatorIsOutOfRange;
                }
            }

            // Succesfully parsed

            time = new MusicalTime(bars, beats, fraction);
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
