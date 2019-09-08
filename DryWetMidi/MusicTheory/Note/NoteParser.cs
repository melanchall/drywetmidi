using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class NoteParser
    {
        #region Constants

        private const string NoteNameGroupName = "n";
        private const string OctaveGroupName = "o";

        private static readonly string OctaveGroup = ParsingUtilities.GetIntegerNumberGroup(OctaveGroupName);

        private static readonly string[] Patterns = NoteNameParser.GetPatterns()
                                                                  .Select(p => $@"(?<{NoteNameGroupName}>{p})\s*{OctaveGroup}")
                                                                  .ToArray();

        private const string OctaveIsOutOfRange = "Octave number is out of range.";
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

            NoteName noteName;
            var noteNameParsingResult = NoteNameParser.TryParse(noteNameGroup.Value, out noteName);
            if (noteNameParsingResult.Status != ParsingStatus.Parsed)
                return noteNameParsingResult;

            int octaveNumber;
            if (!ParsingUtilities.ParseInt(match, OctaveGroupName, Octave.Middle.Number, out octaveNumber))
                return ParsingResult.Error(OctaveIsOutOfRange);

            if (!NoteUtilities.IsNoteValid(noteName, octaveNumber))
                return ParsingResult.Error(NoteIsOutOfRange);

            note = Note.Get(noteName, octaveNumber);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
