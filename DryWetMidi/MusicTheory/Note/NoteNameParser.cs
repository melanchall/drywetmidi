using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class NoteNameParser
    {
        #region Constants

        private const string NoteNameGroupName = "n";
        private const string AccidentalGroupName = "a";

        private static readonly string NoteNameGroup = $"(?<{NoteNameGroupName}>C|D|E|F|G|A|B)";
        private static readonly string AccidentalGroup = $"(?<{AccidentalGroupName}>{Regex.Escape(Note.SharpShortString)}|{Note.SharpLongString})";

        private static readonly string[] Patterns = new[]
        {
            $@"{NoteNameGroup}\s*{AccidentalGroup}",
            $@"{NoteNameGroup}",
        };

        private const string NoteNameIsInvalid = "Note's name is invalid.";

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

            var noteNameGroup = match.Groups[NoteNameGroupName];
            var noteNameString = noteNameGroup.Value;

            var accidentalGroup = match.Groups[AccidentalGroupName];
            if (accidentalGroup.Success)
            {
                var accidental = accidentalGroup.Value;
                accidental = accidental.Replace(Note.SharpShortString, Note.SharpLongString);
                accidental = char.ToUpper(accidental[0]) + accidental.Substring(1);
                noteNameString += accidental;
            }

            if (!Enum.TryParse(noteNameString, out noteName))
                return ParsingResult.Error(NoteNameIsInvalid);

            return ParsingResult.Parsed;
        }

        #endregion
    }
}
