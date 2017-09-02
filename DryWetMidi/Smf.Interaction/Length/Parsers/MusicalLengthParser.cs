using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MusicalLengthParser
    {
        #region Methods

        internal static ParsingResult TryParse(string input, out MusicalLength length)
        {
            length = null;

            var result = FractionParser.TryParse(input, out var fraction);
            if (result.Status == ParsingStatus.Parsed)
                length = new MusicalLength(fraction);

            return result;
        }

        #endregion
    }
}
