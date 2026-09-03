using Melanchall.DryWetMidi.Common;
using System;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class NoteParser : SimpleParser<Note>
    {
        internal override Regex[] GetRegexes()
        {
            throw new System.NotImplementedException();
        }

        protected override Note ParseInternal(string input)
        {
            var span = input.AsSpan();

            if (span.Length < 2)
                ThrowInvalidFormatError();

            if (!char.IsDigit(span[^1]))
                ThrowInvalidFormatError();

            var octaveNumberPartLength = 1;
            if (span[^2] == '-' || span[^2] == '+')
                octaveNumberPartLength++;

            if (!int.TryParse(span[^octaveNumberPartLength..], out var octaveNumber))
                ThrowInvalidFormatError();

            var noteName = MusicTheoryParsers.NoteNameParser.Parse(span[..^octaveNumberPartLength].Trim().ToString());
            if (!NoteUtilities.IsNoteValid(noteName, octaveNumber))
                ThrowError("Note is out of range.");

            return Note.Get(noteName, octaveNumber);
        }
    }
}
