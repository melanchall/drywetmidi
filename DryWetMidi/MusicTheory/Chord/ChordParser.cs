using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class ChordParser
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

        internal static ParsingResult TryParse(string input, out Chord chord)
        {
            chord = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns, ignoreCase: false);
            if (match == null)
                return ParsingResult.NotMatched;

            var rootNoteNameGroup = match.Groups[RootNoteNameGroupName];

            NoteName rootNoteName;
            var rootNoteNameParsingResult = NoteNameParser.TryParse(rootNoteNameGroup.Value, out rootNoteName);
            if (rootNoteNameParsingResult.Status != ParsingStatus.Parsed)
                return rootNoteNameParsingResult;

            //

            NoteName? bassNoteName = null;
            var bassNoteNameGroup = match.Groups[BassNoteNameGroupName];
            if (bassNoteNameGroup.Success)
            {
                NoteName actualBassNoteName;
                var bassNoteNameParsingResult = NoteNameParser.TryParse(bassNoteNameGroup.Value, out actualBassNoteName);
                if (bassNoteNameParsingResult.Status != ParsingStatus.Parsed)
                    return bassNoteNameParsingResult;

                bassNoteName = actualBassNoteName;
            }

            var notesNames = ChordsNamesTable.GetChordNotesNames(rootNoteName, match.Groups[ChordCharacteristicsGroupName].Value, bassNoteName);
            if (!notesNames.Any())
                return ParsingResult.Error(ChordCharacteristicIsUnknown);
            
            chord = new Chord(notesNames);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
