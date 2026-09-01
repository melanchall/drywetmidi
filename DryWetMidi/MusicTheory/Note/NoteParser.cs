using Melanchall.DryWetMidi.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class NoteParser : SimpleParser<Note>
    {
        #region Constants

        private const string NoteNameGroupName = "n";
        private const string OctaveGroupName = "o";

        private static readonly string OctaveGroup = GetIntegerNumberGroup(OctaveGroupName);

        private const string OctaveIsOutOfRange = "Octave number is out of range.";
        private const string NoteIsOutOfRange = "Note is out of range.";

        #endregion

        #region Methods

        internal override Regex[] GetRegexes() => MusicTheoryParsers
            .NoteNameParser
            .GetPatterns()
            .Select(p => new Regex($@"^(?<{NoteNameGroupName}>{p})\s*{OctaveGroup}$", RegexOptions.Compiled | RegexOptions.IgnoreCase))
            .ToArray();

        protected override Note ParseInternal(string input)
        {
            var match = Match(input);
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
