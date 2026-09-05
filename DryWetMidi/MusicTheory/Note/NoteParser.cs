using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class NoteParser : SimpleParser<Note>
    {
        protected override Note ParseInternal(ReadOnlySpan<char> input)
        {
            var (noteName, length) = MusicTheoryParsers.NoteNameParser.TryReadNoteName(input);
            if (noteName == null)
                ThrowInvalidFormatError();

            if (!int.TryParse(input[length..].Trim(), out var octaveNumber))
                ThrowInvalidFormatError();

            if (!NoteUtilities.IsNoteValid(noteName.Value, octaveNumber))
                ThrowError("Note is out of range.");

            return Note.Get(noteName.Value, octaveNumber);
        }
    }
}
