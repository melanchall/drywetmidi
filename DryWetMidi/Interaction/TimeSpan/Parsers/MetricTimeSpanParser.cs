using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MetricTimeSpanParser : SimpleParser<MetricTimeSpan>
    {
        internal override bool TryParseInternal(ReadOnlySpan<char> input, out MetricTimeSpan result, out string? error)
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
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                if (colonCount == 1)
                {
                    if (!TryReadNextSegment(ref input, out minutes) ||
                        !TryReadNextSegment(ref input, out seconds))
                    {
                        error = "Input string has invalid format.";
                        result = default!;
                        return false;
                    }
                }
                else if (colonCount == 2)
                {
                    if (!TryReadNextSegment(ref input, out hours) ||
                        !TryReadNextSegment(ref input, out minutes) ||
                        !TryReadNextSegment(ref input, out seconds))
                    {
                        error = "Input string has invalid format.";
                        result = default!;
                        return false;
                    }
                }
                else
                {
                    if (!TryReadNextSegment(ref input, out hours) ||
                        !TryReadNextSegment(ref input, out minutes) ||
                        !TryReadNextSegment(ref input, out seconds) ||
                        !TryReadNextSegment(ref input, out milliseconds))
                    {
                        error = "Input string has invalid format.";
                        result = default!;
                        return false;
                    }
                }

                result = new MetricTimeSpan(hours, minutes, seconds, milliseconds);
                error = null;
                return true;
            }

            //

            ReadOnlySpan<char> remaining = input;

            var hoursParsed = false;
            var minutesParsed = false;
            var secondsParsed = false;
            var millisecondsParsed = false;

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
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                if (!int.TryParse(remaining[..unitStartIndex], out int value))
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                remaining = remaining[unitStartIndex..];
                remaining = remaining.TrimStart();

                if (remaining.StartsWith("ms", StringComparison.OrdinalIgnoreCase))
                {
                    if (millisecondsParsed)
                    {
                        error = "Input string has invalid format.";
                        result = default!;
                        return false;
                    }

                    millisecondsParsed = true;

                    milliseconds = value;
                    remaining = remaining[2..];
                }
                else if (remaining.StartsWith("h", StringComparison.OrdinalIgnoreCase))
                {
                    if (hoursParsed)
                    {
                        error = "Input string has invalid format.";
                        result = default!;
                        return false;
                    }

                    hoursParsed = true;

                    hours = value;
                    remaining = remaining[1..];
                }
                else if (remaining.StartsWith("m", StringComparison.OrdinalIgnoreCase))
                {
                    if (minutesParsed)
                    {
                        error = "Input string has invalid format.";
                        result = default!;
                        return false;
                    }

                    minutesParsed = true;

                    minutes = value;
                    remaining = remaining[1..];
                }
                else if (remaining.StartsWith("s", StringComparison.OrdinalIgnoreCase))
                {
                    if (secondsParsed)
                    {
                        error = "Input string has invalid format.";
                        result = default!;
                        return false;
                    }

                    secondsParsed = true;

                    seconds = value;
                    remaining = remaining[1..];
                }
                else
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }
            }

            result = new MetricTimeSpan(hours, minutes, seconds, milliseconds);
            error = null;
            return true;
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
