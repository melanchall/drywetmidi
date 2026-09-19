using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MusicalTimeSpanParser : SimpleParser<MusicalTimeSpan>
    {
        internal override bool TryParseInternal(ReadOnlySpan<char> input, out MusicalTimeSpan result, out string? error)
        {
            var (numerator, denominator) = input[0] switch
            {
                'w' => (1L, 1L),
                'h' => (1L, 2L),
                'q' => (1L, 4L),
                'e' => (1L, 8L),
                's' => (1L, 16L),
                _ => (0L, 0L)
            };

            if (numerator > 0)
            {
                input = input.Slice(1).Trim();
            }
            else
            {
                var dividerIndex = input.IndexOf('/');
                if (dividerIndex < 0)
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                numerator = 1;
                if (dividerIndex > 0 && !long.TryParse(input.Slice(0, dividerIndex).Trim(), out numerator))
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                var i = dividerIndex + 1;

                for (; i < input.Length && char.IsWhiteSpace(input[i]); i++) { }
                for (; i < input.Length && char.IsDigit(input[i]); i++) { }

                if (!long.TryParse(input.Slice(dividerIndex + 1, i - dividerIndex - 1).Trim(), out denominator))
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                input = input.Slice(i).Trim();
            }

            //

            if (denominator == 0)
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            if (input.IsEmpty)
            {
                result = new MusicalTimeSpan(numerator, denominator);
                error = null;
                return true;
            }

            //

            var (tupletNotesCount, tupletSpaceSize) = input[0] switch
            {
                't' => (3, 2),
                'd' => (2, 3),
                _ => (0, 0)
            };

            if (tupletNotesCount > 0)
            {
                input = input.Slice(1).Trim();
            }
            else if (input[0] == '[')
            {
                var endIndex = input.IndexOf(']');
                if (endIndex < 0 || endIndex == 1)
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                var tupletSpan = input.Slice(1, endIndex - 1).Trim();
                var tupletDividerIndex = tupletSpan.IndexOf(':');
                if (tupletDividerIndex < 0)
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                if (!int.TryParse(tupletSpan.Slice(0, tupletDividerIndex).Trim(), out tupletNotesCount) || tupletNotesCount < 1)
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                if (!int.TryParse(tupletSpan.Slice(tupletDividerIndex + 1).Trim(), out tupletSpaceSize) || tupletSpaceSize < 1)
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                input = input.Slice(endIndex + 1).Trim();
            }
            else
            {
                tupletNotesCount = 1;
                tupletSpaceSize = 1;
            }

            //

            if (input.IsEmpty)
            {
                result = new MusicalTimeSpan(numerator, denominator).Tuplet(tupletNotesCount, tupletSpaceSize);
                error = null;
                return true;
            }

            //

            var dotsCount = 0;
            while (dotsCount < input.Length && input[dotsCount] == '.')
            {
                dotsCount++;
            }

            //

            if (dotsCount < input.Length)
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            //

            result = new MusicalTimeSpan(numerator, denominator).Dotted(dotsCount).Tuplet(tupletNotesCount, tupletSpaceSize);
            error = null;
            return true;
        }
    }
}
