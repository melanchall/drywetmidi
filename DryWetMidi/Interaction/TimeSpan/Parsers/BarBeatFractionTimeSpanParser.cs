using Melanchall.DryWetMidi.Common;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class BarBeatFractionTimeSpanParser : SimpleParser<BarBeatFractionTimeSpan>
    {
        private static readonly NumberFormatInfo CommaSeparatorFormat = new()
        {
            NumberDecimalSeparator = ","
        };

        private static readonly NumberFormatInfo DotSeparatorFormat = new()
        {
            NumberDecimalSeparator = "."
        };

        internal override Regex[] GetRegexes()
        {
            throw new NotImplementedException();
        }

        protected override BarBeatFractionTimeSpan ParseInternal(string input)
        {
            var bars = 0.0;
            var beats = 0.0;

            var span = input.AsSpan();

            var separatorIndex = span.IndexOf('_');
            if (separatorIndex == -1)
                ThrowInvalidFormatError();

            var barsSpan = span[..separatorIndex].Trim();
            var beatsSpan = span[(separatorIndex + 1)..].Trim();

            if (!double.TryParse(barsSpan, NumberStyles.AllowDecimalPoint, CommaSeparatorFormat, out bars))
                ThrowInvalidFormatError();

            if (!double.TryParse(beatsSpan, NumberStyles.AllowDecimalPoint, CommaSeparatorFormat, out beats) &&
                !double.TryParse(beatsSpan, NumberStyles.AllowDecimalPoint, DotSeparatorFormat, out beats))
                ThrowInvalidFormatError();

            return new BarBeatFractionTimeSpan(bars, beats);
        }
    }
}
