using Melanchall.DryWetMidi.Common;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ChordParser : SimpleParser<Chord>
    {
        #region Constants

        private const string ChordCharacteristicsGroupName = "cc";

        public const string ChordCharacteristicsGroup = $"(?<{ChordCharacteristicsGroupName}>.*?)";

        #endregion

        #region Methods

        internal override Regex[] GetRegexes()
        {
            throw new NotImplementedException();
        }

        protected override Chord ParseInternal(string input)
        {
            var span = input.AsSpan();

            NoteName? bassNoteName = null;

            var bassNoteMarkerIndex = span.LastIndexOf('/');
            if (bassNoteMarkerIndex >= 0 && MusicTheoryParsers.NoteNameParser.TryParse(span[(bassNoteMarkerIndex + 1)..].Trim().ToString(), out var bassNoteNameX))
                bassNoteName = bassNoteNameX;

            var mainPart = bassNoteName != null
                ? span[..bassNoteMarkerIndex].Trim().ToString()
                : input;

            var chordCharacteristic = ChordsNamesTable
                .NamesDefinitions
                .SelectMany(d => d.Names)
                .OrderByDescending(n => n.Length)
                .FirstOrDefault(d => mainPart.EndsWith(d)) ?? string.Empty;

            var rootNoteName = MusicTheoryParsers.NoteNameParser.Parse(mainPart.Substring(0, mainPart.Length - chordCharacteristic.Length));
            var notesNames = ChordsNamesTable.GetChordNotesNames(rootNoteName, chordCharacteristic, bassNoteName);

            return new Chord(notesNames);
        }

        #endregion
    }
}
