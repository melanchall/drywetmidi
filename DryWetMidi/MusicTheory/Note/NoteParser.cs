using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class NoteParser : SimpleParser<Note>
    {
        internal override bool TryParseInternal(ReadOnlySpan<char> input, out Note result, out string? error)
        {
            var (noteName, length) = MusicTheoryParsers.NoteNameParser.TryReadNoteName(input);
            if (noteName == null)
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            if (!int.TryParse(input[length..].Trim(), out var octaveNumber))
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            if (!NoteUtilities.IsNoteValid(noteName.Value, octaveNumber))
            {
                error = "Note is out of range.";
                result = default!;
                return false;
            }

            result = Note.Get(noteName.Value, octaveNumber);
            error = null;
            return true;
        }
    }
}
