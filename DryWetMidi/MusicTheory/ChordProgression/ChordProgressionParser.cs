using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ChordProgressionParser : ParameterizedParser<ChordProgression, Scale>
    {
        #region Constants

        private const char PartsDelimiter = '-';
        
        private static readonly Dictionary<char, int> RomanMap = new ()
        {
            ['I'] = 1,
            ['V'] = 5,
            ['X'] = 10,
            ['L'] = 50,
            ['C'] = 100,
            ['D'] = 500,
            ['M'] = 1000
        };

        #endregion

        #region Methods

        internal override Regex[] GetRegexes()
        {
            throw new NotImplementedException();
        }

        protected override ChordProgression ParseInternal(string input, Scale parameter)
        {
            var parts = input.Split(new[] { PartsDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            var chords = new List<Chord>();

            foreach (var x in parts)
            {
                var part = x.Trim();
                var b = part[0] == 'b';

                var span = part.AsSpan().Slice(b ? 1 : 0).Trim();
                var romanLength = 0;

                while (romanLength < span.Length && RomanMap.ContainsKey(span[romanLength]))
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
            }

            return new ChordProgression(chords);
        }

        private static int RomanToInteger(string roman)
        {
            var number = 0;

            for (int i = 0; i < roman.Length; i++)
            {
                if (i + 1 < roman.Length && RomanMap[roman[i]] < RomanMap[roman[i + 1]])
                    number -= RomanMap[roman[i]];
                else
                    number += RomanMap[roman[i]];
            }

            return number;
        }

        #endregion
    }
}
