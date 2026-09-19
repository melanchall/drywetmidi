using Melanchall.DryWetMidi.Common;
using System;
using System.Globalization;

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

        internal override bool TryParseInternal(ReadOnlySpan<char> input, out BarBeatFractionTimeSpan result, out string? error)
        {
            var bars = 0.0;
            var beats = 0.0;

            var separatorIndex = input.IndexOf('_');
            if (separatorIndex == -1)
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            var barsSpan = input[..separatorIndex].Trim();
            var beatsSpan = input[(separatorIndex + 1)..].Trim();

            if (!double.TryParse(barsSpan, NumberStyles.AllowDecimalPoint, CommaSeparatorFormat, out bars))
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            if (!double.TryParse(beatsSpan, NumberStyles.AllowDecimalPoint, CommaSeparatorFormat, out beats) &&
                !double.TryParse(beatsSpan, NumberStyles.AllowDecimalPoint, DotSeparatorFormat, out beats))
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            result = new BarBeatFractionTimeSpan(bars, beats);
            error = null;
            return true;
        }
    }
}
