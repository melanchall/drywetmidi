using System;

namespace Melanchall.DryWetMidi.Common
{
    internal abstract class SimpleParser<T> : Parser
    {
        public T Parse(ReadOnlySpan<char> input)
        {
            ThrowIfArgument.IsEmptyOrWhiteSpaceString(nameof(input), input, "Input");

            return ParseInternal(input.Trim());
        }

        public bool TryParse(ReadOnlySpan<char> input, out T result)
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
