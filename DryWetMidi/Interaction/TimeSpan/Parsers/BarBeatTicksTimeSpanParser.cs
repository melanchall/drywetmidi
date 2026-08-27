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

        protected override BarBeatTicksTimeSpan ParseInternal(ReadOnlySpan<char> input)
        {
            var bars = 0.0;
            var beats = 0.0;
            var ticks = 0;

            var firstDot = input.IndexOf('.');
            if (firstDot == -1)
                ThrowInvalidFormatError();

            var secondDot = input[(firstDot + 1)..].IndexOf('.');
            if (secondDot == -1) 
                ThrowInvalidFormatError();

            secondDot = firstDot + 1 + secondDot;

            var barsSpan = input[..firstDot].Trim();
            var beatsSpan = input[(firstDot + 1)..secondDot].Trim();
            var ticksSpan = input[(secondDot + 1)..].Trim();

            if (!double.TryParse(barsSpan, NumberStyles.AllowDecimalPoint, CommaSeparatorFormat, out bars))
                ThrowInvalidFormatError();

            if (!double.TryParse(beatsSpan, NumberStyles.AllowDecimalPoint, CommaSeparatorFormat, out beats))
                ThrowInvalidFormatError();

            if (!int.TryParse(ticksSpan, out ticks))
                ThrowInvalidFormatError();

            return new BarBeatTicksTimeSpan(bars, beats, ticks);
        }
    }
}
