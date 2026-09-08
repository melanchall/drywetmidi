using System;

namespace Melanchall.DryWetMidi.Common
{
    internal abstract class ParameterizedParser<T, TParam> : Parser
    {
        public T Parse(ReadOnlySpan<char> input, TParam parameter)
        {
            ThrowIfArgument.IsEmptyOrWhiteSpaceString(nameof(input), input, "Input");

            return ParseInternal(input.Trim(), parameter);
        }

        public bool TryParse(ReadOnlySpan<char> input, TParam parameter, out T result)
        {
            try
            {
                result = Parse(input, parameter);
                return true;
            }
            catch
            {
                result = default!;
                return false;
            }
        }

        protected abstract T ParseInternal(ReadOnlySpan<Char> input, TParam parameter);
    }
}
