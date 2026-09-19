using System;

namespace Melanchall.DryWetMidi.Common
{
    internal abstract class SimpleParser<T> : Parser
    {
        public T Parse(ReadOnlySpan<char> input)
        {
            ThrowIfArgument.IsEmptyOrWhiteSpaceString(nameof(input), input, "Input");

            if (!TryParseInternal(input.Trim(), out var result, out var error))
                throw new FormatException(error ?? "Input string has invalid format.");

            return result;
        }

        public bool TryParse(ReadOnlySpan<char> input, out T result)
        {
            if (input.IsEmpty || input.Trim().IsEmpty)
            {
                result = default!;
                return false;
            }

            return TryParseInternal(input.Trim(), out result, out _);
        }

        internal abstract bool TryParseInternal(
            ReadOnlySpan<char> input,
            out T result,
            out string? error);
    }
}
