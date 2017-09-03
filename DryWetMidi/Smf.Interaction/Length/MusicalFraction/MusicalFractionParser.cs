using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MusicalFractionParser
    {
        #region Constants

        private static readonly Dictionary<string, Tuple<int, int>> Fractions = new Dictionary<string, Tuple<int, int>>
        {
            ["w"] = Tuple.Create(1, 1),
            ["h"] = Tuple.Create(1, 2),
            ["q"] = Tuple.Create(1, 4),
            ["e"] = Tuple.Create(1, 8),
            ["s"] = Tuple.Create(1, 16),
        };

        private static readonly Dictionary<string, Tuple<int, int>> Tuplets = new Dictionary<string, Tuple<int, int>>
        {
            ["t"] = Tuple.Create(3, 2),
            ["d"] = Tuple.Create(2, 3),
        };

        private const string NumeratorGroupName = "n";
        private const string DenominatorGroupName = "d";
        private const string FractionMnemonicGroupName = "fm";

        private const string TupletNotesCountGroupName = "tn";
        private const string TupletSpaceSizeGroupName = "ts";
        private const string TupletMnemonicGroupName = "tm";

        private const string DotsGroupName = "dt";

        private static readonly string FractionGroup = $@"(?<{NumeratorGroupName}>\d+)?\/(?<{DenominatorGroupName}>\d+)";
        private static readonly string FractionMnemonicGroup = GetMnemonicGroup(FractionMnemonicGroupName, Fractions.Keys);

        private static readonly string TupletGroup = $@"\[\s*(?<{TupletNotesCountGroupName}>\d+)\s*\:\s*(?<{TupletSpaceSizeGroupName}>\d+)\s*\]";
        private static readonly string TupletMnemonicGroup = GetMnemonicGroup(TupletMnemonicGroupName, Tuplets.Keys);

        private static readonly string DotsGroup = $@"(?<{DotsGroupName}>\.+)";

        private static readonly string[] Patterns = new[]
        {
            $@"({FractionGroup}|{FractionMnemonicGroup})\s*({TupletGroup}|{TupletMnemonicGroup})?\s*{DotsGroup}?"
        };

        private const string NumeratorIsOutOfRange = "Numerator is out of range.";
        private const string DenominatorIsOutOfRange = "Denominator is out of range.";
        private const string TupletNotesCountIsOutOfRange = "Tuplet's notes count is out of range.";
        private const string TupletSpaceSizeIsOutOfRange = "Tuplet's space size is out of range.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out Fraction fraction)
        {
            fraction = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            // Fraction

            if (!ParsingUtilities.ParseLong(match, NumeratorGroupName, 1, out var numerator))
                return new ParsingResult(NumeratorIsOutOfRange);

            if (!ParsingUtilities.ParseLong(match, DenominatorGroupName, 1, out var denominator))
                return new ParsingResult(DenominatorIsOutOfRange);

            var fractionMnemonicGroup = match.Groups[FractionMnemonicGroupName];
            if (fractionMnemonicGroup.Success)
            {
                var fractionDefinition = Fractions[fractionMnemonicGroup.Value];
                numerator = fractionDefinition.Item1;
                denominator = fractionDefinition.Item2;
            }

            // Tuplet

            if (!ParsingUtilities.ParseInt(match, TupletNotesCountGroupName, 1, out var tupletNotesCount))
                return new ParsingResult(TupletNotesCountIsOutOfRange);

            if (!ParsingUtilities.ParseInt(match, TupletSpaceSizeGroupName, 1, out var tupletSpaceSize))
                return new ParsingResult(TupletSpaceSizeIsOutOfRange);

            var tupletMnemonicGroup = match.Groups[TupletMnemonicGroupName];
            if (tupletMnemonicGroup.Success)
            {
                var tupletDefinition = Tuplets[tupletMnemonicGroup.Value];
                tupletNotesCount = tupletDefinition.Item1;
                tupletSpaceSize = tupletDefinition.Item2;
            }

            // Dots

            var dotsGroup = match.Groups[DotsGroupName];
            var dots = dotsGroup.Success
                ? dotsGroup.Value.Length
                : 0;

            //

            fraction = MusicalFraction.CreateDottedTuplet(numerator, denominator, dots, tupletNotesCount, tupletSpaceSize);
            return ParsingResult.Parsed;
        }

        private static string GetMnemonicGroup(string groupName, IEnumerable<string> mnemonics)
        {
            return $@"(?<{groupName}>[{string.Join(string.Empty, mnemonics)}])";
        }

        #endregion
    }
}
