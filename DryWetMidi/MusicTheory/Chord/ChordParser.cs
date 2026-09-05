using Melanchall.DryWetMidi.Common;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ChordParser : SimpleParser<Chord>
    {
        internal override Regex[] GetRegexes()
        {
            throw new NotImplementedException();
        }

        protected override Chord ParseInternal(string input)
        {
            var span = input.AsSpan().Trim();

            var (rootNoteName, rootNoteNamePartLength) = MusicTheoryParsers.NoteNameParser.TryReadNoteName(span);
            if (rootNoteName == null)
                ThrowInvalidFormatError();

            NoteName? bassNoteName = null;

            var bassNoteMarkerIndex = span.LastIndexOf('/');
            if (bassNoteMarkerIndex >= 0)
                (bassNoteName, _) = MusicTheoryParsers.NoteNameParser.TryReadNoteName(span.Slice(bassNoteMarkerIndex + 1).Trim());

            var chordCharacteristic = bassNoteName != null
                ? span.Slice(rootNoteNamePartLength, bassNoteMarkerIndex - rootNoteNamePartLength).Trim()
                : span.Slice(rootNoteNamePartLength).Trim();

            var notesNames = ChordsNamesTable.GetChordNotesNames(rootNoteName.Value, chordCharacteristic.ToString(), bassNoteName);
            if (!notesNames.Any())
                ThrowError("Chord characteristic is unknown.");

            return new Chord(notesNames);
        }
    }
}
