namespace Melanchall.DryWetMidi.Common
{
    internal sealed class ShortByteParser : Parser<byte>
    {
        private readonly byte _minValue;
        private readonly byte _maxValue;

        public ShortByteParser(byte minValue, byte maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }

        #region Methods

        protected override byte ParseInternal(string? input)
        {
            if (!byte.TryParse(input?.Trim(), out var result) || result < _minValue || result > _maxValue)
                ThrowError("Number is invalid or is out of valid range.");

            return result;
        }

        #endregion
    }
}
