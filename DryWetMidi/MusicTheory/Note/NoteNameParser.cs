using System;
using System.Linq;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class NoteNameParser : SimpleParser<NoteName>
    {
        #region Constants

        private const string NoteLetterGroupName = "n";
        private const string AccidentalGroupName = "a";

        private const string NoteNameGroup = $"(?<{NoteLetterGroupName}>C|D|E|F|G|A|B)";
        private const string AccidentalGroup = $"((?<{AccidentalGroupName}>{Note.SharpShortString}|{Note.SharpLongString}|{Note.FlatShortString}|{Note.FlatLongString})\\s*)*";

        #endregion

        #region Methods

        internal string[] GetPatterns() => new[]
        {
            $@"{NoteNameGroup}\s*{AccidentalGroup}",
        };

        internal override Regex[] GetRegexes() => GetPatterns()
            .Select(p => new Regex($@"^{p}$", RegexOptions.Compiled | RegexOptions.IgnoreCase))
            .ToArray();

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
