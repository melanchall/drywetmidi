namespace Melanchall.DryWetMidi.Common
{
    internal static class ShortByteParser
    {
        #region Methods

        internal static ParsingResult TryParse(string input, byte minValue, byte maxValue, out byte result)
        {
            result = default(byte);

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            if (!byte.TryParse(input.Trim(), out var tmpResult) || tmpResult < minValue || tmpResult > maxValue)
                return ParsingResult.Error("Number is invalid or is out of valid range.");

            result = tmpResult;
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
