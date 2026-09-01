using Melanchall.DryWetMidi.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class BarBeatTicksTimeSpanParser : SimpleParser<BarBeatTicksTimeSpan>
    {
        #region Constants

        private const string BarsGroupName = "bars";
        private const string BeatsGroupName = "beats";
        private const string TicksGroupName = "ticks";

        private static readonly string BarsGroup = GetNonnegativeDoubleNumberGroup(BarsGroupName, ',');
        private static readonly string BeatsGroup = GetNonnegativeDoubleNumberGroup(BeatsGroupName, ',');
        private static readonly string TicksGroup = GetNonnegativeIntegerNumberGroup(TicksGroupName);

        private static readonly string Divider = Regex.Escape(".");

        private const string BarsIsOutOfRange = "Bars number is out of range.";
        private const string BeatsIsOutOfRange = "Beats number is out of range.";
        private const string TicksIsOutOfRange = "Ticks number is out of range.";

        #endregion

        #region Methods

        internal override Regex[] GetRegexes() => new[]
        {
            new Regex($@"^{BarsGroup}\s*{Divider}\s*{BeatsGroup}\s*{Divider}\s*{TicksGroup}$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };

        protected override BarBeatTicksTimeSpan ParseInternal(string input)
        {
            var match = Match(input);
            if (match == null)
                ThrowInvalidFormatError();

            if (!ParseNonnegativeDouble(match, BarsGroupName, 0, new[] { ',' }, out var bars))
                ThrowError(BarsIsOutOfRange);

            if (!ParseNonnegativeDouble(match, BeatsGroupName, 0, new[] { ',' }, out var beats))
                ThrowError(BeatsIsOutOfRange);

            if (!ParseNonnegativeLong(match, TicksGroupName, 0, out var ticks))
                ThrowError(TicksIsOutOfRange);

            return new BarBeatTicksTimeSpan(bars, beats, ticks);
        }

        #endregion
    }
}
