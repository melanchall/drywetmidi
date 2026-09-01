using Melanchall.DryWetMidi.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ChordParser : SimpleParser<Chord>
    {
        #region Constants

        private const string RootNoteNameGroupName = "rn";
        private const string BassNoteNameGroupName = "bn";
        private const string ChordCharacteristicsGroupName = "cc";

        public static readonly string ChordCharacteristicsGroup = $"(?<{ChordCharacteristicsGroupName}>.*?)";
        private static readonly string RootNoteNameGroup = $"(?<{RootNoteNameGroupName}>{string.Join("|", MusicTheoryParsers.NoteNameParser.GetPatterns())})";
        private static readonly string BassNoteNameGroup = $"(?<{BassNoteNameGroupName}>{string.Join("|", MusicTheoryParsers.NoteNameParser.GetPatterns())})";

        private const string ChordCharacteristicIsUnknown = "Chord characteristic is unknown.";

        #endregion

        #region Methods

        internal override Regex[] GetRegexes() => new[]
        {
            new Regex($@"(?i:{RootNoteNameGroup}){ChordCharacteristicsGroup}((\/(?i:{BassNoteNameGroup}))|$)", RegexOptions.Compiled),
        };

        protected override Chord ParseInternal(string input)
        {
            var match = Match(input);
            if (match == null)
                ThrowInvalidFormatError();

            var rootNoteNameGroup = match.Groups[RootNoteNameGroupName];

            var rootNoteName = MusicTheoryParsers.NoteNameParser.Parse(rootNoteNameGroup.Value);

            //

            NoteName? bassNoteName = null;
            var bassNoteNameGroup = match.Groups[BassNoteNameGroupName];
            if (bassNoteNameGroup.Success)
                bassNoteName = MusicTheoryParsers.NoteNameParser.Parse(bassNoteNameGroup.Value);

            var notesNames = ChordsNamesTable.GetChordNotesNames(rootNoteName, match.Groups[ChordCharacteristicsGroupName].Value, bassNoteName);
            if (!notesNames.Any())
                ThrowError(ChordCharacteristicIsUnknown);

            return new Chord(notesNames);
        }

        #endregion
    }
}
