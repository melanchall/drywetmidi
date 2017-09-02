using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MidiLengthParser
    {
        #region Methods

        internal static ParsingResult TryParse(string input, out MidiLength length)
        {
            length = null;

            var result = MidiTimeParser.TryParse(input, out var time);
            if (result.Status == ParsingStatus.Parsed)
                length = new MidiLength(time);

            return result;
        }

        #endregion
    }
}
