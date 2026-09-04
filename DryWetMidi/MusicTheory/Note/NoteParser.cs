using Melanchall.DryWetMidi.Common;
using System;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class NoteParser : SimpleParser<Note>
    {
        internal override Regex[] GetRegexes()
        {
            throw new NotImplementedException();
        }

        protected override Note ParseInternal(string input)
        {
            var span = input.AsSpan();

            var (noteName, length) = MusicTheoryParsers.NoteNameParser.TryReadNoteName(span.Trim());
            if (noteName == null)
                ThrowInvalidFormatError();

            if (!int.TryParse(span[length..].Trim(), out var octaveNumber))
                ThrowInvalidFormatError();

            if (!NoteUtilities.IsNoteValid(noteName.Value, octaveNumber))
                ThrowError("Note is out of range.");

            return Note.Get(noteName.Value, octaveNumber);
        }
    }
}
