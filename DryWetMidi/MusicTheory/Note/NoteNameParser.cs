using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class NoteNameParser : SimpleParser<NoteName>
    {
        public (NoteName? NoteName, int Length) TryReadNoteName(ReadOnlySpan<char> input)
        {
            if (input.Length == 0)
                return (null, 0);

            var noteName = (char.ToLower(input[0])) switch
            {
                'a' => NoteName.A,
                'b' => NoteName.B,
                'c' => NoteName.C,
                'd' => NoteName.D,
                'e' => NoteName.E,
                'f' => NoteName.F,
                'g' => NoteName.G,
                _ => (NoteName?)null,
            };

            if (noteName == null)
                return (null, 0);

            var noteBaseNumber = (int)noteName;
            var i = 1;
            var trailingSpacesCount = 0;

            while (i < input.Length)
            {
                if (char.IsWhiteSpace(input[i]))
                {
                    i++;
                    trailingSpacesCount++;
                    continue;
                }

                if (input[i] == '#')
                {
                    noteBaseNumber++;
                    i++;
                    trailingSpacesCount = 0;
                    continue;
                }
                if (input[i] == 'b' || input[i] == 'B')
                {
                    noteBaseNumber--;
                    i++;
                    trailingSpacesCount = 0;
                    continue;
                }

                var slice = input.Slice(i);

                if (slice.StartsWith(Note.SharpLongString, StringComparison.OrdinalIgnoreCase))
                {
                    noteBaseNumber++;
                    i += Note.SharpLongString.Length;
                    trailingSpacesCount = 0;
                }
                else if (slice.StartsWith(Note.FlatLongString, StringComparison.OrdinalIgnoreCase))
                {
                    noteBaseNumber--;
                    i += Note.FlatLongString.Length;
                    trailingSpacesCount = 0;
                }
                else
                {
                    break;
                }
            }

            noteBaseNumber %= Octave.OctaveSize;
            if (noteBaseNumber < 0)
                noteBaseNumber = Octave.OctaveSize + noteBaseNumber;

            return ((NoteName)noteBaseNumber, i - trailingSpacesCount);
        }

        internal override bool TryParseInternal(ReadOnlySpan<char> input, out NoteName result, out string? error)
        {
            var (noteName, length) = TryReadNoteName(input);
            if (noteName == null || length != input.Length)
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            result = noteName.Value;
            error = null;
            return true;
        }
    }
}
