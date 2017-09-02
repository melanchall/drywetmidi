using Melanchall.DryWetMidi.Common;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MusicalTimeParser
    {
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

        private const string BarsIsOutOfRange = "Bars number is out of range.";
        private const string BeatsIsOutOfRange = "Beats number is out of range.";
        private const string FractionNotMatched = "Input string has invalid fraction format.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out MusicalTime time)
        {
            time = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            // Parse bars, beats

            if (!ParsingUtilities.ParseInt(match, BarsGroupName, 0, out var bars))
                return new ParsingResult(BarsIsOutOfRange);

            if (!ParsingUtilities.ParseInt(match, BeatsGroupName, 0, out var beats))
                return new ParsingResult(BeatsIsOutOfRange);

            // Parse fraction

            var fraction = Fraction.ZeroFraction;
            var fractionGroup = match.Groups[FractionGroupName];
            if (fractionGroup.Success)
            {
                var fractionParsingResult = FractionParser.TryParse(fractionGroup.Value, out fraction);
                switch (fractionParsingResult.Status)
                {
                    case ParsingStatus.EmptyInputString:
                    case ParsingStatus.NotMatched:
                        return new ParsingResult(FractionNotMatched);

                    case ParsingStatus.FormatError:
                        return new ParsingResult(fractionParsingResult.Error);
                }
            }

            // Succesfully parsed

            time = new MusicalTime(bars, beats, fraction);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
