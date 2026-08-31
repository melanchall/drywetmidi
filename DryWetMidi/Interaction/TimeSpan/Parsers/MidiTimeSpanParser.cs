using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MidiTimeSpanParser : SimpleParser<MidiTimeSpan>
    {
        #region Constants

        private const string TimeSpanGroupName = "ts";

        private static readonly string TimeSpanGroup = GetNonnegativeIntegerNumberGroup(TimeSpanGroupName);

        private static readonly string[] Patterns = new[]
        {
            TimeSpanGroup,
        };

        private const string OutOfRange = "Time span is out of range.";

        #endregion

        #region Methods

        protected override MidiTimeSpan ParseInternal(string input)
        {
            var match = Match(input, Patterns);
            if (match == null)
                ThrowInvalidFormatError();

            if (!ParseNonnegativeLong(match, TimeSpanGroupName, 0, out var midiTimeSpan))
                ThrowError(OutOfRange);

            return new MidiTimeSpan(midiTimeSpan);
        }

        #endregion
    }
}
