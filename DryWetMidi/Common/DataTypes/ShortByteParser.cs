using System;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class ShortByteParser : SimpleParser<byte>
    {
        private readonly byte _minValue;
        private readonly byte _maxValue;

        public ShortByteParser(byte minValue, byte maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }

        internal override Regex[] GetRegexes()
        {
            throw new NotImplementedException();
        }

        protected override byte ParseInternal(ReadOnlySpan<char> input)
        {
            if (!byte.TryParse(input, out var result) || result < _minValue || result > _maxValue)
                ThrowError("Number is invalid or is out of valid range.");

            return result;
        }
    }
}
