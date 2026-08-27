using Melanchall.DryWetMidi.Common;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ChordParser : SimpleParser<Chord>
    {
        protected override Chord ParseInternal(ReadOnlySpan<char> input)
        {
            var (rootNoteName, rootNoteNamePartLength) = MusicTheoryParsers.NoteNameParser.TryReadNoteName(input);
            if (rootNoteName == null)
                ThrowInvalidFormatError();

            NoteName? bassNoteName = null;

            var bassNoteMarkerIndex = input.LastIndexOf('/');
            if (bassNoteMarkerIndex >= 0)
                (bassNoteName, _) = MusicTheoryParsers.NoteNameParser.TryReadNoteName(input.Slice(bassNoteMarkerIndex + 1).Trim());

            var chordCharacteristic = bassNoteName != null
                ? input.Slice(rootNoteNamePartLength, bassNoteMarkerIndex - rootNoteNamePartLength).Trim()
                : input.Slice(rootNoteNamePartLength).Trim();

            var notesNames = ChordsNamesTable.GetChordNotesNames(rootNoteName.Value, chordCharacteristic.ToString(), bassNoteName);
            if (!notesNames.Any())
                ThrowError("Chord characteristic is unknown.");

            return new Chord(notesNames);
        }
    }
}
