using Melanchall.DryWetMidi.Common;
using System.Linq;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ChordParser : SimpleParser<Chord>
    {
        #region Constants

        private const string RootNoteNameGroupName = "rn";
        private const string BassNoteNameGroupName = "bn";
        private const string ChordCharacteristicsGroupName = "cc";

        public static readonly string ChordCharacteristicsGroup = $"(?<{ChordCharacteristicsGroupName}>.*?)";
        private static readonly string RootNoteNameGroup = $"(?<{RootNoteNameGroupName}>{string.Join("|", NoteNameParser.GetPatterns())})";
        private static readonly string BassNoteNameGroup = $"(?<{BassNoteNameGroupName}>{string.Join("|", NoteNameParser.GetPatterns())})";

        private static readonly string[] Patterns = new[]
        {
            $@"(?i:{RootNoteNameGroup}){ChordCharacteristicsGroup}((\/(?i:{BassNoteNameGroup}))|$)",
        };

        private const string ChordCharacteristicIsUnknown = "Chord characteristic is unknown.";

        #endregion

        #region Methods

        protected override Chord ParseInternal(string input)
        {
            var match = Match(input, Patterns, ignoreCase: false);
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
