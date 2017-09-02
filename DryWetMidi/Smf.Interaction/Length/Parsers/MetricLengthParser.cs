using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MetricLengthParser
    {
        #region Methods

        internal static ParsingResult TryParse(string input, out MetricLength length)
        {
            length = null;

            var result = MetricTimeParser.TryParse(input, out var time);
            if (result.Status == ParsingStatus.Parsed)
                length = new MetricLength(time);

            return result;
        }

        #endregion
    }
}
