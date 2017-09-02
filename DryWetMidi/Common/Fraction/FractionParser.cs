using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Common
{
    internal static class FractionParser
    {
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

            // /denominator -> 1/denominator
            $@"{Divider}\s*{DenominatorGroup}",
        };

        private const string NumeratorIsOutOfRange = "Numerator is out of range.";
        private const string DenominatorIsOutOfRange = "Denominator is out of range.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out Fraction fraction)
        {
            fraction = Fraction.ZeroFraction;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            if (!ParsingUtilities.ParseLong(match, NumeratorGroupName, 1, out var numerator))
                return new ParsingResult(NumeratorIsOutOfRange);

            if (!ParsingUtilities.ParseLong(match, DenominatorGroupName, 1, out var denominator))
                return new ParsingResult(DenominatorIsOutOfRange);

            fraction = new Fraction(numerator, denominator);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
