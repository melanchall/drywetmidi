using Melanchall.DryWetMidi.Common;
using System;
using System.Globalization;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class BarBeatTicksTimeSpanParser : SimpleParser<BarBeatTicksTimeSpan>
    {
        private static readonly NumberFormatInfo CommaSeparatorFormat = new()
        {
            NumberDecimalSeparator = ","
        };

        internal override bool TryParseInternal(ReadOnlySpan<char> input, out BarBeatTicksTimeSpan result, out string? error)
        {
            var bars = 0.0;
            var beats = 0.0;
            var ticks = 0L;

            var firstDot = input.IndexOf('.');
            if (firstDot == -1)
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            var secondDot = input[(firstDot + 1)..].IndexOf('.');
            if (secondDot == -1) 
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            secondDot = firstDot + 1 + secondDot;

            var barsSpan = input[..firstDot].Trim();
            var beatsSpan = input[(firstDot + 1)..secondDot].Trim();
            var ticksSpan = input[(secondDot + 1)..].Trim();

            if (!double.TryParse(barsSpan, NumberStyles.AllowDecimalPoint, CommaSeparatorFormat, out bars))
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            if (!double.TryParse(beatsSpan, NumberStyles.AllowDecimalPoint, CommaSeparatorFormat, out beats))
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            if (!long.TryParse(ticksSpan, out ticks))
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            result = new BarBeatTicksTimeSpan(bars, beats, ticks);
            error = null;
            return true;
        }
    }
}
