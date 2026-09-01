using Melanchall.DryWetMidi.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class BarBeatFractionTimeSpanParser : SimpleParser<BarBeatFractionTimeSpan>
    {
        #region Constants

        private const string BarsGroupName = "bars";
        private const string BeatsGroupName = "beats";

        private static readonly string BarsGroup = GetNonnegativeDoubleNumberGroup(BarsGroupName, ',');
        private static readonly string BeatsGroup = GetNonnegativeDoubleNumberGroup(BeatsGroupName, '.', ',');

        private static readonly string Divider = Regex.Escape("_");

        private const string BarsIsOutOfRange = "Bars number is out of range.";
        private const string BeatsIsOutOfRange = "Beats number is out of range.";

        #endregion

        #region Methods

        internal override Regex[] GetRegexes() => new[]
        {
            new Regex($@"^{BarsGroup}\s*{Divider}\s*{BeatsGroup}$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };

        protected override BarBeatFractionTimeSpan ParseInternal(string input)
        {
            var match = Match(input);
            if (match == null)
                ThrowInvalidFormatError();

            if (!ParseNonnegativeDouble(match, BarsGroupName, 0, new[] { ',' }, out var bars))
                ThrowError(BarsIsOutOfRange);

            if (!ParseNonnegativeDouble(match, BeatsGroupName, 0, new[] { '.', ',' }, out var beats))
                ThrowError(BeatsIsOutOfRange);

            return new BarBeatFractionTimeSpan(bars, beats);
        }

        #endregion
    }
}
