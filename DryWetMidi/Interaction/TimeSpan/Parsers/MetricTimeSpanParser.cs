using Melanchall.DryWetMidi.Common;
using System;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MetricTimeSpanParser : SimpleParser<MetricTimeSpan>
    {
        internal override Regex[] GetRegexes()
        {
            throw new NotImplementedException();
        }

        protected override MetricTimeSpan ParseInternal(ReadOnlySpan<char> input)
        {
            var hours = 0;
            var minutes = 0;
            var seconds = 0;
            var milliseconds = 0;

            var colonCount = 0;
            foreach (var c in input)
            {
                if (c == ':')
                    colonCount++;
            }

            if (colonCount > 0)
            {
                if (colonCount < 1 || colonCount > 3)
                    ThrowInvalidFormatError();

                if (colonCount == 1)
                {
                    if (!TryReadNextSegment(ref input, out minutes) ||
                        !TryReadNextSegment(ref input, out seconds))
                        ThrowInvalidFormatError();
                }
                else if (colonCount == 2)
                {
                    if (!TryReadNextSegment(ref input, out hours) ||
                        !TryReadNextSegment(ref input, out minutes) ||
                        !TryReadNextSegment(ref input, out seconds))
                        ThrowInvalidFormatError();
                }
                else
                {
                    if (!TryReadNextSegment(ref input, out hours) ||
                        !TryReadNextSegment(ref input, out minutes) ||
                        !TryReadNextSegment(ref input, out seconds) ||
                        !TryReadNextSegment(ref input, out milliseconds))
                        ThrowInvalidFormatError();
                }

                return new MetricTimeSpan(hours, minutes, seconds, milliseconds);
            }

            //

            ReadOnlySpan<char> remaining = input;

            while (!remaining.IsEmpty)
            {
                remaining = remaining.TrimStart();
                if (remaining.IsEmpty)
                    break;

                var unitStartIndex = 0;
                while (unitStartIndex < remaining.Length && char.IsDigit(remaining[unitStartIndex]))
                {
                    unitStartIndex++;
                }

                if (unitStartIndex == 0)
                    ThrowInvalidFormatError();

                if (!int.TryParse(remaining[..unitStartIndex], out int value))
                    ThrowInvalidFormatError();

                remaining = remaining[unitStartIndex..];
                remaining = remaining.TrimStart();

                if (remaining.StartsWith("ms", StringComparison.OrdinalIgnoreCase))
                {
                    milliseconds = value;
                    remaining = remaining[2..];
                }
                else if (remaining.StartsWith("h", StringComparison.OrdinalIgnoreCase))
                {
                    hours = value;
                    remaining = remaining[1..];
                }
                else if (remaining.StartsWith("m", StringComparison.OrdinalIgnoreCase))
                {
                    minutes = value;
                    remaining = remaining[1..];
                }
                else if (remaining.StartsWith("s", StringComparison.OrdinalIgnoreCase))
                {
                    seconds = value;
                    remaining = remaining[1..];
                }
                else
                    ThrowInvalidFormatError();
            }

            return new MetricTimeSpan(hours, minutes, seconds, milliseconds);
        }

        private static bool TryReadNextSegment(ref ReadOnlySpan<char> remaining, out int value)
        {
            var index = remaining.IndexOf(':');
            var segment = index >= 0 ? remaining[..index] : remaining;

            if (!int.TryParse(segment, out value))
                return false;

            remaining = index >= 0
                ? remaining[(index + 1)..]
                : ReadOnlySpan<char>.Empty;
            
            return true;
        }
    }
}
