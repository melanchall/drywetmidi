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

        internal static ParsingResult TryParse(string s, out Fraction fraction)
        {
            fraction = null;

            if (string.IsNullOrWhiteSpace(s))
                return ParsingResult.InputStringIsNullOrWhiteSpace;

            s = s.Trim();

            var match = _regex.Match(s);
            if (!match.Success)
                return ParsingResult.NotMatched;

            if (!ParseFractionPart(match, NumeratorGroupName, out var numerator))
                return ParsingResult.NumeratorIsOutOfRange;

            if (!ParseFractionPart(match, DenominatorGroupName, out var denominator))
                return ParsingResult.DenominatorIsOutOfRange;

            fraction = new Fraction(numerator, denominator);
            return ParsingResult.Parsed;
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
