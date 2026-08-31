namespace Melanchall.DryWetMidi.Common
{
    internal abstract class ParameterizedParser<T, TParam> : Parser
    {
        public T Parse(string? input, TParam parameter)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(input), input, "Input");

            return ParseInternal(input!, parameter);
        }

        public bool TryParse(string? input, TParam parameter, out T result)
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

        protected abstract T ParseInternal(string input, TParam parameter);
    }
}
