using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ChordProgressionParser : ParameterizedParser<ChordProgression, Scale>
    {
        private const string RomanDigits = "IVXLCDM";

        internal override bool TryParseInternal(ReadOnlySpan<char> input, Scale parameter, out ChordProgression result, out string? error)
        {
            var chords = new List<Chord>();

            while (input.Length > 0)
            {
                var delimiterIndex = input.IndexOf('-');

                var part = delimiterIndex >= 0
                    ? input.Slice(0, delimiterIndex).Trim()
                    : input;

                if (part.IsEmpty)
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                var b = part[0] == 'b';

                var span = part.Slice(b ? 1 : 0).Trim();
                var romanLength = 0;

                while (romanLength < span.Length && RomanDigits.Contains(span[romanLength]))
                {
                    romanLength++;
                }

                if (romanLength == 0)
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }

                var degree = RomanToInteger(span.Slice(0, romanLength));
                var rootNoteName = parameter.GetStep(degree - 1);

                if (b)
                    rootNoteName = (NoteName)(((int)rootNoteName + Octave.OctaveSize - 1) % Octave.OctaveSize);

                var chordString =
                    rootNoteName +
                    span.Slice(romanLength).ToString();

                if (!MusicTheoryParsers.ChordParser.TryParseInternal(chordString, out var chord, out error))
                {
                    result = default!;
                    return false;
                }

                chords.Add(chord);

                if (delimiterIndex < 0)
                    break;

                input = input.Slice(delimiterIndex + 1).Trim();
                if (input.IsEmpty)
                {
                    error = "Input string has invalid format.";
                    result = default!;
                    return false;
                }
            }

            result = new ChordProgression(chords);
            error = null;
            return true;
        }

        private static int RomanToInteger(ReadOnlySpan<char> roman)
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
