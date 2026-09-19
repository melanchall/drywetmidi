using Melanchall.DryWetMidi.Common;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ChordParser : SimpleParser<Chord>
    {
        internal override bool TryParseInternal(ReadOnlySpan<char> input, out Chord result, out string? error)
        {
            var (rootNoteName, rootNoteNamePartLength) = MusicTheoryParsers.NoteNameParser.TryReadNoteName(input);
            if (rootNoteName == null)
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            NoteName? bassNoteName = null;

            var bassNoteMarkerIndex = input.LastIndexOf('/');
            if (bassNoteMarkerIndex >= 0)
            {
                var bassPart = input.Slice(bassNoteMarkerIndex + 1).Trim();

                var (candidateBassNoteName, length) = MusicTheoryParsers.NoteNameParser.TryReadNoteName(bassPart);

                if (candidateBassNoteName != null && length == bassPart.Length)
                    bassNoteName = candidateBassNoteName;
            }

            var chordCharacteristic = bassNoteName != null
                ? input.Slice(rootNoteNamePartLength, bassNoteMarkerIndex - rootNoteNamePartLength).Trim()
                : input.Slice(rootNoteNamePartLength).Trim();

            var notesNames = ChordsNamesTable.GetChordNotesNames(rootNoteName.Value, chordCharacteristic, bassNoteName);
            if (!notesNames.Any())
            {
                error = "Chord characteristic is unknown.";
                result = default!;
                return false;
            }

            result = new Chord(notesNames);
            error = null;
            return true;
        }
    }
}
