using System;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class NoteParser
    {
        #region Constants

        private const string NoteNameGroupName = "n";
        private const string AccidentalGroupName = "a";
        private const string OctaveGroupName = "o";

        private static readonly string NoteNameGroup = $"(?<{NoteNameGroupName}>C|D|E|F|G|A|B)";
        private static readonly string AccidentalGroup = $"(?<{AccidentalGroupName}>{Regex.Escape(Note.SharpShortString)}|{Note.SharpLongString})";
        private static readonly string OctaveGroup = ParsingUtilities.GetNumberGroup(OctaveGroupName);

        private static readonly string[] Patterns = new[]
        {
            $@"{NoteNameGroup}\s*{AccidentalGroup}\s*{OctaveGroup}",
            $@"{NoteNameGroup}\s*{OctaveGroup}",
        };

        private const string OctaveIsOutOfRange = "Octave number is out of range.";
        private const string NoteNameIsInvalid = "Note's name is invalid.";
        private const string NoteIsOutOfRange = "Note is out of range.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out Note note)
        {
            note = null;

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

            int octaveNumber;
            if (!ParsingUtilities.ParseInt(match, OctaveGroupName, Octave.Middle.Number, out octaveNumber))
                return ParsingResult.Error(OctaveIsOutOfRange);

            NoteName noteName;
            if (!Enum.TryParse(noteNameString, out noteName))
                return ParsingResult.Error(NoteNameIsInvalid);

            if (!NoteUtilities.IsNoteValid(noteName, octaveNumber))
                return ParsingResult.Error(NoteIsOutOfRange);

            note = Note.Get(noteName, octaveNumber);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
