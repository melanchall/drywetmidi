using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class MidiTimeSpanParser
    {
        #region Constants

        private const string TimeSpanGroupName = "ts";

        private static readonly string TimeSpanGroup = ParsingUtilities.GetNonnegativeIntegerNumberGroup(TimeSpanGroupName);

        private static readonly string[] Patterns = new[]
        {
            TimeSpanGroup,
        };

        private const string OutOfRange = "Time span is out of range.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out MidiTimeSpan timeSpan)
        {
            timeSpan = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            long midiTimeSpan;
            if (!ParsingUtilities.ParseNonnegativeLong(match, TimeSpanGroupName, 0, out midiTimeSpan))
                return ParsingResult.Error(OutOfRange);

            timeSpan = new MidiTimeSpan(midiTimeSpan);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
