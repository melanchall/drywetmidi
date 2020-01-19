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
            var uniqueNotesNames = new HashSet<NoteName>(notesNames);
            var rootNoteName = uniqueNotesNames.First();

            var result = new List<string>();
            var intervals = ChordUtilities.GetIntervalsFromRootNote(uniqueNotesNames).Select(i => i.HalfSteps).ToArray();

            foreach (var nameDefinition in NamesDefinitions)
            {
                var matched = false;

                foreach (var definitionIntervals in nameDefinition.Intervals)
                {
                    var intervalsX = intervals;

                    if (definitionIntervals[0] != 0)
                    {
                        // TODO: process omitted root
                    }
                    else
                    {
                        intervalsX = new[] { 0 }.Concat(intervals).ToArray();
                    }

                    var subMatched = intervalsX.Length == definitionIntervals.Length;

                    for (int i = 0; i < definitionIntervals.Length && subMatched; i++)
                    {
                        var interval = definitionIntervals[i];
                        if (intervalsX[i] != interval)
                            subMatched = false;
                    }

                    matched |= subMatched;
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
