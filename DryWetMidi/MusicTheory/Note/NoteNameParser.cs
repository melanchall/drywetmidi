using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class NoteNameParser
    {
        #region Constants

        private const string NoteLetterGroupName = "n";
        private const string AccidentalGroupName = "a";

        private static readonly string NoteNameGroup = $"(?<{NoteLetterGroupName}>C|D|E|F|G|A|B)";
        private static readonly string AccidentalGroup = $"((?<{AccidentalGroupName}>{Regex.Escape(Note.SharpShortString)}|{Note.SharpLongString}|{Note.FlatShortString}|{Note.FlatLongString})\\s*)+?";

        private static readonly string[] Patterns = new[]
        {
            $@"{NoteNameGroup}\s*{AccidentalGroup}",
            NoteNameGroup,
        };

        #endregion

        #region Methods

        internal static IEnumerable<string> GetPatterns()
        {
            return Patterns;
        }

        internal static ParsingResult TryParse(string input, out NoteName noteName)
        {
            noteName = default(NoteName);

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

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

            noteName = (NoteName)noteBaseNumber;
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
