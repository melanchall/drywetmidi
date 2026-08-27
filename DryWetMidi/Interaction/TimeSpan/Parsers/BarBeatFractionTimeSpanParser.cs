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

        protected override BarBeatFractionTimeSpan ParseInternal(ReadOnlySpan<char> input)
        {
            var bars = 0.0;
            var beats = 0.0;

            var separatorIndex = input.IndexOf('_');
            if (separatorIndex == -1)
                ThrowInvalidFormatError();

            var barsSpan = input[..separatorIndex].Trim();
            var beatsSpan = input[(separatorIndex + 1)..].Trim();

            if (!double.TryParse(barsSpan, NumberStyles.AllowDecimalPoint, CommaSeparatorFormat, out bars))
                ThrowInvalidFormatError();

            if (!double.TryParse(beatsSpan, NumberStyles.AllowDecimalPoint, CommaSeparatorFormat, out beats) &&
                !double.TryParse(beatsSpan, NumberStyles.AllowDecimalPoint, DotSeparatorFormat, out beats))
                ThrowInvalidFormatError();

            return new BarBeatFractionTimeSpan(bars, beats);
        }
    }
}
