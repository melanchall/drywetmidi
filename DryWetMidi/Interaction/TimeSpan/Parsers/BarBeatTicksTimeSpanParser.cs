using Melanchall.DryWetMidi.Common;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class BarBeatTicksTimeSpanParser : SimpleParser<BarBeatTicksTimeSpan>
    {
        private static readonly NumberFormatInfo CommaSeparatorFormat = new()
        {
            NumberDecimalSeparator = ","
        };

        internal override Regex[] GetRegexes()
        {
            throw new NotImplementedException();
        }

        protected override BarBeatTicksTimeSpan ParseInternal(string input)
        {
            var bars = 0.0;
            var beats = 0.0;
            var ticks = 0;

            var span = input.AsSpan();

            var firstDot = span.IndexOf('.');
            if (firstDot == -1)
                ThrowInvalidFormatError();

            var secondDot = span[(firstDot + 1)..].IndexOf('.');
            if (secondDot == -1) 
                ThrowInvalidFormatError();

            secondDot = firstDot + 1 + secondDot;

            var barsSpan = span[..firstDot].Trim();
            var beatsSpan = span[(firstDot + 1)..secondDot].Trim();
            var ticksSpan = span[(secondDot + 1)..].Trim();

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
