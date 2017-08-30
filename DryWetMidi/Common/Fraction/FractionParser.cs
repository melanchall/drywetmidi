using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Common
{
    internal static class FractionParser
    {
        #region Nested types

        internal enum ParsingResult
        {
            Parsed,

            InputStringIsNullOrWhiteSpace,
            NotMatched,
            NumeratorIsOutOfRange,
            DenominatorIsOutOfRange,
        }

        #endregion

        #region Constants

        private const string NumeratorGroupName = "n";
        private const string DenominatorGroupName = "d";

        private static readonly string NumeratorGroup = ParsingUtilities.GetNumberGroup(NumeratorGroupName);
        private static readonly string DenominatorGroup = ParsingUtilities.GetNumberGroup(DenominatorGroupName);

        private static readonly string Divider = Regex.Escape("/");

        private static readonly string[] Patterns = new[]
        {
            // numerator/denominator -> numerator/denominator
            $@"{NumeratorGroup}\s*{Divider}\s*{DenominatorGroup}",

            // numerator -> numerator/1
            $@"{NumeratorGroup}",

            // /denominator -> 1/denominator
            $@"{Divider}\s*{DenominatorGroup}",
        };

        private static readonly Dictionary<ParsingResult, string> FormatExceptionMessages =
            new Dictionary<ParsingResult, string>
            {
                [ParsingResult.NotMatched] = "Input string has invalid fraction format.",
                [ParsingResult.NumeratorIsOutOfRange] = "Numerator is out of range.",
                [ParsingResult.DenominatorIsOutOfRange] = "Denominator is out of range."
    };

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out Fraction fraction)
        {
            fraction = Fraction.ZeroFraction;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.InputStringIsNullOrWhiteSpace;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            if (!ParsingUtilities.ParseLong(match, NumeratorGroupName, 1, out var numerator))
                return ParsingResult.NumeratorIsOutOfRange;

            if (!ParsingUtilities.ParseLong(match, DenominatorGroupName, 1, out var denominator))
                return ParsingResult.DenominatorIsOutOfRange;

            fraction = new Fraction(numerator, denominator);
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
