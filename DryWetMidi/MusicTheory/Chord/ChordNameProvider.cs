using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class ChordNameProvider
    {
        private sealed class NameDefinition
        {
            public NameDefinition(int[][] intervals, params string[] names)
            {
                Intervals = intervals;
                Names = names;
            }

            public int[][] Intervals { get; }

            public string[] Names { get; }
        }

        private static readonly NameDefinition[] NamesDefinitions = new[]
        {
            new NameDefinition(new[] { new[] { 0, 4, 7 } }, "maj", "M", string.Empty),
            new NameDefinition(new[] { new[] { 0, 3, 7 } }, "min", "m"),
            new NameDefinition(new[] { new[] { 0, 5, 7 } }, "sus4"),
            new NameDefinition(new[] { new[] { 0, 2, 7 } }, "sus2"),
            new NameDefinition(new[] { new[] { 0, 4, 6 } }, "b5"),
            new NameDefinition(new[] { new[] { 0, 3, 6 } }, "dim"),
            new NameDefinition(new[] { new[] { 0, 4, 8 } }, "aug"),
            new NameDefinition(new[] { new[] { 0, 3, 7, 9 } }, "min6", "m6"),
            new NameDefinition(new[] { new[] { 0, 4, 7, 9 } }, "maj6", "M6", "6"),
            new NameDefinition(new[] { new[] { 0, 4, 7, 10 } }, "7"),
            new NameDefinition(new[] { new[] { 0, 5, 7, 10 } }, "7sus4"),
            new NameDefinition(new[] { new[] { 0, 2, 7, 10 } }, "7sus2"),
            new NameDefinition(new[] { new[] { 0, 3, 7, 10 } }, "min7", "m7"),
            new NameDefinition(new[] { new[] { 0, 3, 7, 10, 14 }, new[] { 0, 3, 10, 14 }, new[] { 3, 10, 14 }, new[] { 3, 7, 10, 14 } }, "min9", "min7(9)", "m9", "m7(9)"),
            new NameDefinition(new[] { new[] { 0, 3, 7, 10, 14, 17 }, new[] { 0, 3, 10, 14, 17 }, new[] { 3, 10, 14, 17 }, new[] { 3, 7, 10, 14, 17 } }, "min11", "min7(9,11)", "m11", "m7(9,11)"),
            new NameDefinition(new[] { new[] { 0, 4, 7, 11 } }, "maj7"),
            new NameDefinition(new[] { new[] { 0, 4, 7, 11, 14 }, new[] { 0, 4, 11, 14 }, new[] { 4, 11, 14 }, new[] { 4, 7, 11, 14 } }, "maj7(9)", "M7(9)"),
            new NameDefinition(new[] { new[] { 0, 4, 7, 11, 14, 18 }, new[] { 0, 4, 11, 14, 18 }, new[] { 4, 11, 14, 18 }, new[] { 4, 7, 11, 14, 18 } }, "maj7(#11)", "M7(#11)"),
            new NameDefinition(new[] { new[] { 0, 4, 7, 11, 21 }, new[] { 0, 4, 11, 21 }, new[] { 4, 11, 21 }, new[] { 4, 7, 11, 21 } }, "maj7(13)", "M7(13)"),
            new NameDefinition(new[] { new[] { 0, 4, 7, 11, 14, 21 }, new[] { 0, 4, 11, 14, 21 }, new[] { 4, 11, 14, 21 }, new[] { 4, 7, 11, 14, 21 } }, "maj7(9,13)", "M7(9,13)"),
            new NameDefinition(new[] { new[] { 0, 4, 8, 11 } }, "maj7#5", "M7#5"),
            new NameDefinition(new[] { new[] { 0, 4, 8, 11, 14 }, new[] { 4, 8, 11, 14 } }, "maj7#5(9)", "M7#5(9)"),
            new NameDefinition(new[] { new[] { 0, 3, 7, 11 } }, "minMaj7", "mM7"),
            new NameDefinition(new[] { new[] { 0, 3, 7, 11, 14 }, new[] { 0, 3, 11, 14 }, new[] { 3, 11, 14 }, new[] { 3, 7, 11, 14 } }, "minMaj7(9)", "mM7(9)"),
            new NameDefinition(new[] { new[] { 0, 7 } }, "5"),
            new NameDefinition(new[] { new[] { 0, 4, 6, 10 } }, "7b5", "dom7dim5", "7dim5"),
            new NameDefinition(new[] { new[] { 0, 3, 6, 10 } }, "ø", "ø7", "m7b5", "min7dim5", "m7dim5", "min7b5", "m7b5"),
            new NameDefinition(new[] { new[] { 0, 4, 8, 10 } }, "aug7"),
            new NameDefinition(new[] { new[] { 0, 3, 6, 9 } }, "dim7"),
            new NameDefinition(new[] { new[] { 0, 4, 7, 14 } }, "add9"),
            new NameDefinition(new[] { new[] { 0, 3, 7, 14 } }, "minAdd9", "mAdd9"),
            new NameDefinition(new[] { new[] { 0, 4, 7, 9, 14 }, new[] { 4, 7, 9, 14 }, new[] { 0, 4, 9, 14 }, new[] { 4, 9, 14 } }, "maj6(9)", "6(9)", "6/9", "M6/9", "M6(9)"),
            new NameDefinition(new[] { new[] { 0, 3, 7, 9, 14 }, new[] { 3, 7, 9, 14 }, new[] { 0, 3, 9, 14 }, new[] { 3, 9, 14 } }, "min6(9)", "m6(9)", "m6/9", "min6/9"),
            new NameDefinition(new[] { new[] { 0, 4, 7, 10, 14 } }, "9"),
            new NameDefinition(new[] { new[] { 0, 2, 7, 10, 14 } }, "9sus2"),
            new NameDefinition(new[] { new[] { 0, 5, 7, 10, 14 } }, "9sus4"),
            new NameDefinition(new[] { new[] { 0, 4, 7, 10, 14, 17 } }, "11")
        };

        public static IList<string> GetChordName(ICollection<NoteName> notesNames)
        {
            var firstNoteName = notesNames.First();

            var result = GetChordNameInternal(notesNames);
            result.AddRange(GetChordNameInternal(notesNames.Skip(1).ToArray()).Select(n => $"{n}/{firstNoteName}"));

            result.Sort((x, y) => x.Length - y.Length);
            return result;
        }

        private static List<string> GetChordNameInternal(ICollection<NoteName> notesNames)
        {
            var rootNoteName = notesNames.First();

            var result = new List<string>();
            var intervals = ChordUtilities.GetIntervalsFromRootNote(notesNames).Select(i => i.HalfSteps).ToArray();

            foreach (var nameDefinition in NamesDefinitions)
            {
                var matched = false;

                foreach (var definitionIntervals in nameDefinition.Intervals)
                {
                    var intervalsX = intervals;

                    if (definitionIntervals[0] != 0)
                    {
                        // TODO: process omitted root
                        continue;
                    }
                    else
                    {
                        intervalsX = new[] { 0 }.Concat(intervals).ToArray();
                    }

                    var subMatched = intervalsX.Length >= definitionIntervals.Length;
                    var j = 0;

                    for (int i = 0; i < definitionIntervals.Length && i < intervalsX.Length && subMatched; i++, j++)
                    {
                        var interval = definitionIntervals[i];
                        if (intervalsX[i] != interval && !intervalsX.Contains(interval - 12) && !intervalsX.Contains(interval - 24))
                            subMatched = false;
                    }

                    for (; j < intervalsX.Length && subMatched; j++)
                    {
                        if (!intervalsX.Contains(intervalsX[j] - 12) && !intervalsX.Contains(intervalsX[j] - 24))
                            subMatched = false;
                    }

                    matched |= subMatched && j >= intervalsX.Length;
                    if (matched)
                        break;
                }

                if (matched)
                    result.AddRange(nameDefinition.Names.Select(n => $"{rootNoteName.ToString().Replace(Note.SharpLongString, Note.SharpShortString)}{n}"));
            }

            return result;
        }
    }
}
