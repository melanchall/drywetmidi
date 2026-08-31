using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class NoteParser : SimpleParser<Note>
    {
        #region Constants

        private const string NoteNameGroupName = "n";
        private const string OctaveGroupName = "o";

        private static readonly string OctaveGroup = GetIntegerNumberGroup(OctaveGroupName);

        private static readonly string[] Patterns = NoteNameParser.GetPatterns()
                                                                  .Select(p => $@"(?<{NoteNameGroupName}>{p})\s*{OctaveGroup}")
                                                                  .ToArray();

        private const string OctaveIsOutOfRange = "Octave number is out of range.";
        private const string NoteIsOutOfRange = "Note is out of range.";

        #endregion

        #region Methods

        protected override Note ParseInternal(string input)
        {
            var match = Match(input, Patterns);
            if (match == null)
                ThrowInvalidFormatError();

            var noteNameGroup = match.Groups[NoteNameGroupName];

            var noteName = MusicTheoryParsers.NoteNameParser.Parse(noteNameGroup.Value);

            if (!ParseInt(match, OctaveGroupName, Octave.Middle.Number, out var octaveNumber))
                ThrowError(OctaveIsOutOfRange);

            if (!NoteUtilities.IsNoteValid(noteName, octaveNumber))
                ThrowError(NoteIsOutOfRange);

            return Note.Get(noteName, octaveNumber);
        }

        #endregion
    }
}
