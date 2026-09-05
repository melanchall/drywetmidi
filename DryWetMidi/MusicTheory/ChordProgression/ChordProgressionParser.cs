using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ChordProgressionParser : ParameterizedParser<ChordProgression, Scale>
    {
        private const string RomanDigits = "IVXLCDM";

        protected override ChordProgression ParseInternal(ReadOnlySpan<char> input, Scale parameter)
        {
            var chords = new List<Chord>();

            while (input.Length > 0)
            {
                var delimiterIndex = input.IndexOf('-');

                var part = delimiterIndex >= 0
                    ? input.Slice(0, delimiterIndex).Trim()
                    : input;

                if (part.IsEmpty)
                    ThrowInvalidFormatError();

                var b = part[0] == 'b';

                var span = part.Slice(b ? 1 : 0).Trim();
                var romanLength = 0;

                while (romanLength < span.Length && RomanDigits.Contains(span[romanLength]))
                {
                    romanLength++;
                }

                var degree = RomanToInteger(span.Slice(0, romanLength).ToString());
                var rootNoteName = parameter.GetStep(degree - 1);

                if (b)
                    rootNoteName = (NoteName)(((int)rootNoteName + Octave.OctaveSize - 1) % Octave.OctaveSize);

                var chordString =
                    rootNoteName +
                    span.Slice(romanLength).ToString();

                var chord = MusicTheoryParsers.ChordParser.Parse(chordString);
                chords.Add(chord);

                if (delimiterIndex < 0)
                    break;

                input = input.Slice(delimiterIndex + 1).Trim();
            }

            return new ChordProgression(chords);
        }

        private static int RomanToInteger(string roman)
        {
            var number = 0;

            for (var i = 0; i < roman.Length; i++)
            {
                if (i + 1 < roman.Length && GetRomanValue(roman[i]) < GetRomanValue(roman[i + 1]))
                    number -= GetRomanValue(roman[i]);
                else
                    number += GetRomanValue(roman[i]);
            }

            return number;
        }

        private static int GetRomanValue(char c) => c switch
        {
            'I' => 1,
            'V' => 5,
            'X' => 10,
            'L' => 50,
            'C' => 100,
            'D' => 500,
            'M' => 1000,
        };
    }
}
