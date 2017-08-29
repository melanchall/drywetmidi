using System;
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

        // Valid formats:
        //     n / d -> n / d
        //     n /   -> n / 1
        //     n     -> n / 1
        //     / d   -> 1 / d
        //
        // / can be one of the following symbols:
        //     /
        //     :
        //     ÷
        private static readonly Regex _regex = new Regex($@"^(?<{NumeratorGroupName}>\d+)?\s*[\/:÷]?\s*(?<{DenominatorGroupName}>\d+)?$");

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out Fraction fraction)
        {
            fraction = Fraction.ZeroFraction;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.InputStringIsNullOrWhiteSpace;

            input = input.Trim();

            var match = _regex.Match(input);
            if (!match.Success)
                return ParsingResult.NotMatched;

            if (!ParseFractionPart(match, NumeratorGroupName, out var numerator))
                return ParsingResult.NumeratorIsOutOfRange;

            if (!ParseFractionPart(match, DenominatorGroupName, out var denominator))
                return ParsingResult.DenominatorIsOutOfRange;

            fraction = new Fraction(numerator, denominator);
            return ParsingResult.Parsed;
        }

        internal static Exception GetException(ParsingResult parsingResult, string inputStringParameterName)
        {
            switch (parsingResult)
            {
                case ParsingResult.InputStringIsNullOrWhiteSpace:
                    return new ArgumentException("Input string is null or contains white-spaces only.", inputStringParameterName);

                case ParsingResult.NotMatched:
                    return new FormatException("Input string has invalid fraction format.");

                case ParsingResult.NumeratorIsOutOfRange:
                    return new FormatException("Numerator is out of range.");

                case ParsingResult.DenominatorIsOutOfRange:
                    return new FormatException("Denominator is out of range.");
            }

            return null;
        }

        private static bool ParseFractionPart(Match match, string groupName, out long value)
        {
            value = 1;

            var group = match.Groups[groupName];
            return !group.Success || long.TryParse(group.Value, out value);
        }

        #endregion
    }
}
