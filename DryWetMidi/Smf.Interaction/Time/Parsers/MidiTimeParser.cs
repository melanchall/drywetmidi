using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal static class MidiTimeParser
    {
        #region Constants

        private const string TimeGroupName = "t";

        private static readonly string TimeGroup = ParsingUtilities.GetNumberGroup(TimeGroupName);

        private static readonly string[] Patterns = new[]
        {
            $@"{TimeGroup}",
        };

        private const string OutOfRange = "Time is out of range.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out MidiTime time)
        {
            time = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            if (!ParsingUtilities.ParseLong(match, TimeGroupName, 0, out var midiTime))
                return new ParsingResult(OutOfRange);

            time = new MidiTime(midiTime);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
