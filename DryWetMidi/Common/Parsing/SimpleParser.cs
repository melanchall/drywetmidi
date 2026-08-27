using System;

namespace Melanchall.DryWetMidi.Common
{
    internal abstract class SimpleParser<T> : Parser
    {
        public T Parse(string? input)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(input), input, "Input");

            return ParseInternal(input.AsSpan().Trim());
        }

        public bool TryParse(string? input, out T result)
        {
            try
            {
                result = Parse(input);
                return true;
            }
            catch
            {
                result = default!;
                return false;
            }
        }

        protected abstract T ParseInternal(ReadOnlySpan<char> input);
    }
}
