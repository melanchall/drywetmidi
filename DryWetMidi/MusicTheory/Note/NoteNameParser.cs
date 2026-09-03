using System;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class NoteNameParser : SimpleParser<NoteName>
    {
        #region Constants

        private const string NoteLetterGroupName = "n";
        private const string AccidentalGroupName = "a";

        private const string NoteNameGroup = $"(?<{NoteLetterGroupName}>[CDEFGAB])";
        private const string AccidentalGroup = $"((?<{AccidentalGroupName}>{Note.SharpShortString}|{Note.SharpLongString}|{Note.FlatShortString}|{Note.FlatLongString})\\s*)*";

        #endregion

        #region Methods

        public (NoteName? NoteName, int Length) TryReadNoteName(ReadOnlySpan<char> input)
        {
            if (input[0] is not >= 'A' and <= 'G')
                return (null, 0);

            var noteBaseNumber = (int)Enum.Parse<NoteName>(input[0].ToString());
            var i = 1;

            while (i < input.Length)
            {
                if (input[i] == ' ')
                {
                    i++;
                    continue;
                }
                if (input[i] == '#')
                {
                    noteBaseNumber++;
                    i++;
                    continue;
                }
                if (input[i] == 'b')
                {
                    noteBaseNumber--;
                    i++;
                    continue;
                }

                var slice = input.Slice(i);

                if (slice.StartsWith(Note.SharpLongString, StringComparison.OrdinalIgnoreCase))
                {
                    noteBaseNumber++;
                    i += Note.SharpLongString.Length;
                }
                else if (slice.StartsWith(Note.FlatLongString, StringComparison.OrdinalIgnoreCase))
                {
                    noteBaseNumber--;
                    i += Note.FlatLongString.Length;
                }
                else
                {
                    break;
                }
            }

            noteBaseNumber %= Octave.OctaveSize;
            if (noteBaseNumber < 0)
                noteBaseNumber = Octave.OctaveSize + noteBaseNumber;

            return ((NoteName)noteBaseNumber, i);
        }

        internal string GetPattern() => $@"{NoteNameGroup}\s*{AccidentalGroup}";

        internal override Regex[] GetRegexes() => new[]
        {
            new Regex($@"^{GetPattern()}$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        protected override NoteName ParseInternal(string input)
        {
            var match = Match(input);
            if (match == null)
                ThrowInvalidFormatError();

            var noteLetterGroup = match.Groups[NoteLetterGroupName];
            var noteBaseNumber = (int)(NoteName)Enum.Parse(typeof(NoteName), noteLetterGroup.Value, true);

            var accidentalGroup = match.Groups[AccidentalGroupName];
            if (accidentalGroup.Success)
            {
                foreach (Capture capture in accidentalGroup.Captures)
                {
                    var accidental = capture.Value;
                    if (string.Equals(accidental, Note.SharpShortString, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(accidental, Note.SharpLongString, StringComparison.OrdinalIgnoreCase))
                        noteBaseNumber++;
                    else if (string.Equals(accidental, Note.FlatShortString, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(accidental, Note.FlatLongString, StringComparison.OrdinalIgnoreCase))
                        noteBaseNumber--;
                }
            }

            noteBaseNumber %= Octave.OctaveSize;
            if (noteBaseNumber < 0)
                noteBaseNumber = Octave.OctaveSize + noteBaseNumber;

            return (NoteName)noteBaseNumber;
        }

        #endregion
    }
}
