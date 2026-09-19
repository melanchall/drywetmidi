using System;

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

        internal override bool TryParseInternal(ReadOnlySpan<char> input, out byte result, out string? error)
        {
            if (!byte.TryParse(input, out result) || result < _minValue || result > _maxValue)
            {
                error = "Number is invalid or is out of valid range.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
