using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class NoteNameParser : SimpleParser<NoteName>
    {
        public (NoteName? NoteName, int Length) TryReadNoteName(ReadOnlySpan<char> input)
        {
            if (input[0] is not >= 'A' and <= 'G')
                return (null, 0);

            var noteBaseNumber = (int)Enum.Parse<NoteName>(input[0].ToString(), true);
            var i = 1;
            var trailingSpacesCount = 0;

            while (i < input.Length)
            {
                if (input[i] == ' ')
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

        protected override NoteName ParseInternal(ReadOnlySpan<char> input)
        {
            var (noteName, length) = TryReadNoteName(input);
            if (noteName == null || length != input.Length)
                ThrowInvalidFormatError();

            return noteName.Value;
        }
    }
}
