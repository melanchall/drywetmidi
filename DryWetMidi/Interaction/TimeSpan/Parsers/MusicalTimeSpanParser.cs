using Melanchall.DryWetMidi.Common;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MusicalTimeSpanParser : SimpleParser<MusicalTimeSpan>
    {
        #region Constants

        private static readonly Dictionary<string, (int, int)> Fractions = new Dictionary<string, (int, int)>
        {
            ["w"] = (1, 1),
            ["h"] = (1, 2),
            ["q"] = (1, 4),
            ["e"] = (1, 8),
            ["s"] = (1, 16),
        };

        private static readonly Dictionary<string, (int TupletNotesCount, int TupletSpaceSize)> Tuplets = new Dictionary<string, (int, int)>
        {
            ["t"] = (3, 2),
            ["d"] = (2, 3),
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

        private const string NumeratorIsOutOfRange = "Numerator is out of range.";
        private const string DenominatorIsOutOfRange = "Denominator is out of range.";
        private const string TupletNotesCountIsOutOfRange = "Tuplet's notes count is out of range.";
        private const string TupletSpaceSizeIsOutOfRange = "Tuplet's space size is out of range.";

        #endregion

        #region Methods

        internal override IEnumerable<string> GetPatterns() => new[]
        {
            $@"({FractionGroup}|{FractionMnemonicGroup})\s*({TupletGroup}|{TupletMnemonicGroup})?\s*{DotsGroup}?",
        };

        protected override MusicalTimeSpan ParseInternal(string input)
        {
            var match = Match(input);
            if (match == null)
                ThrowInvalidFormatError();

            // Fraction

            if (!ParseNonnegativeLong(match, NumeratorGroupName, 1, out var numerator))
                ThrowError(NumeratorIsOutOfRange);

            if (!ParseNonnegativeLong(match, DenominatorGroupName, 1, out var denominator))
                ThrowError(DenominatorIsOutOfRange);

            var fractionMnemonicGroup = match.Groups[FractionMnemonicGroupName];
            if (fractionMnemonicGroup.Success)
            {
                var fraction = Fractions[fractionMnemonicGroup.Value];
                numerator = fraction.Item1;
                denominator = fraction.Item2;
            }

            // Tuplet

            if (!ParseNonnegativeInt(match, TupletNotesCountGroupName, 1, out var tupletNotesCount))
                ThrowError(TupletNotesCountIsOutOfRange);

            if (!ParseNonnegativeInt(match, TupletSpaceSizeGroupName, 1, out var tupletSpaceSize))
                ThrowError(TupletSpaceSizeIsOutOfRange);

            var tupletMnemonicGroup = match.Groups[TupletMnemonicGroupName];
            if (tupletMnemonicGroup.Success)
                (tupletNotesCount, tupletSpaceSize) = Tuplets[tupletMnemonicGroup.Value];

            // Dots

            var dotsGroup = match.Groups[DotsGroupName];
            var dots = dotsGroup.Success
                ? dotsGroup.Value.Length
                : 0;

            //

            return new MusicalTimeSpan(numerator, denominator).Dotted(dots).Tuplet(tupletNotesCount, tupletSpaceSize);
        }

        private static string GetMnemonicGroup(string groupName, IEnumerable<string> mnemonics)
        {
            return $"(?<{groupName}>[{string.Join(string.Empty, mnemonics)}])";
        }

        #endregion
    }
}
