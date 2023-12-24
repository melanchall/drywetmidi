using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class ChordProgressionParser
    {
        #region Constants

        private const char PartsDelimiter = '-';
        
        private const string ScaleDegreeGroupName = "sd";
        private const string AccidentalGroupName = "ac";

        private static readonly string AccidentalGroup = $"(?<{AccidentalGroupName}>b)";
        private static readonly string ScaleDegreeGroup = $"(?<{ScaleDegreeGroupName}>(?i:M{{0,4}}(CM|CD|D?C{{0,3}})(XC|XL|L?X{{0,3}})(IX|IV|V?I{{0,3}})))";

        private static readonly string[] Patterns = new[]
        {
            $@"{AccidentalGroup}?\s*{ScaleDegreeGroup}\s*{ChordParser.ChordCharacteristicsGroup}"
        };

        private static readonly Dictionary<char, int> RomanMap = new Dictionary<char, int>
        {
            ['i'] = 1,
            ['v'] = 5,
            ['x'] = 10,
            ['l'] = 50,
            ['c'] = 100,
            ['d'] = 500,
            ['m'] = 1000
        };

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, Scale scale, out ChordProgression chordProgression)
        {
            chordProgression = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var parts = input.Split(new[] { PartsDelimiter }, System.StringSplitOptions.RemoveEmptyEntries);
            var chords = new List<Chord>();

            foreach (var part in parts)
            {
                var match = ParsingUtilities.Match(part, Patterns, ignoreCase: false);
                if (match == null)
                    return ParsingResult.NotMatched;

                var degreeGroup = match.Groups[ScaleDegreeGroupName];
                var degreeRoman = degreeGroup.Value.ToLower();
                if (string.IsNullOrWhiteSpace(degreeRoman))
                    continue;

                var degree = RomanToInteger(degreeRoman);
                var rootNoteName = scale.GetStep(degree - 1);

                var accidentalGroup = match.Groups[AccidentalGroupName];
                if (accidentalGroup.Success)
                {
                    var accidental = accidentalGroup.Value;
                    if (accidental == "b")
                        rootNoteName = (NoteName)(((int)rootNoteName + Octave.OctaveSize - 1) % Octave.OctaveSize);
                }

                var fullString = match.Value;
                var matchIndex = match.Index;
                var degreeGroupIndex = degreeGroup.Index;
                var chordString =
                    fullString.Substring(0, degreeGroupIndex - matchIndex - (accidentalGroup.Success ? accidentalGroup.Length : 0)) +
                    rootNoteName +
                    fullString.Substring(degreeGroupIndex - matchIndex + degreeGroup.Length);

                Chord chord;
                var chordParsingResult = ChordParser.TryParse(chordString, out chord);
                if (chordParsingResult.Status != ParsingStatus.Parsed)
                    return chordParsingResult;

                chords.Add(chord);
            }

            chordProgression = new ChordProgression(chords);
            return ParsingResult.Parsed;
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
