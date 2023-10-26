using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class ChordsNamesTable
    {
        #region Nested classes

        internal sealed class NameDefinition
        {
            #region Constructor

            public NameDefinition(int[][] intervals, params string[] names)
            {
                Intervals = intervals;
                Names = names;
            }

            #endregion

            #region Properties

            public int[][] Intervals { get; }

            public string[] Names { get; }

            #endregion
        }

        #endregion

        #region Constants

        internal static readonly NameDefinition[] NamesDefinitions = new[]
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
                new NameDefinition(new[] { new[] { 0, 4, 7, 10, 13 } }, "7b9"),
                new NameDefinition(new[] { new[] { 0, 3, 6, 10 } }, "ø", "ø7", "m7b5", "min7dim5", "m7dim5", "min7b5", "m7b5"),
                new NameDefinition(new[] { new[] { 0, 4, 8, 10 } }, "aug7", "7#5", "7+5"),
                new NameDefinition(new[] { new[] { 0, 4, 7, 10, 15 } }, "7#9", "7+9"),
                new NameDefinition(new[] { new[] { 0, 3, 6, 9 } }, "dim7"),
                new NameDefinition(new[] { new[] { 0, 4, 7, 14 } }, "add9"),
                new NameDefinition(new[] { new[] { 0, 3, 7, 14 } }, "minAdd9", "mAdd9"),
                new NameDefinition(new[] { new[] { 0, 4, 7, 9, 14 }, new[] { 4, 7, 9, 14 }, new[] { 0, 4, 9, 14 }, new[] { 4, 9, 14 } }, "maj6(9)", "6(9)", "6/9", "M6/9", "M6(9)"),
                new NameDefinition(new[] { new[] { 0, 3, 7, 9, 14 }, new[] { 3, 7, 9, 14 }, new[] { 0, 3, 9, 14 }, new[] { 3, 9, 14 } }, "min6(9)", "m6(9)", "m6/9", "min6/9"),
                new NameDefinition(new[] { new[] { 0, 4, 7, 10, 14 } }, "9"),
                new NameDefinition(new[] { new[] { 0, 2, 7, 10 } }, "9sus2"),
                new NameDefinition(new[] { new[] { 0, 5, 7, 10, 14 } }, "9sus4"),
                new NameDefinition(new[] { new[] { 0, 4, 7, 10, 14, 17 } }, "11"),
                new NameDefinition(new[] { new[] { 0, 4, 7, 17 } }, "add11"),
                new NameDefinition(new[] { new[] { 0, 4, 7, 10, 18 } }, "7#11", "7+11", "7(#11)", "7aug11"),
            }
            .OrderByDescending(d => d.Intervals.First().Length)
            .ToArray();

        private static readonly int MinIntervalsCount = NamesDefinitions
            .SelectMany(d => d.Intervals.Select(i => i.Length))
            .Min();

        private static readonly int MaxIntervalsCount = NamesDefinitions
            .SelectMany(d => d.Intervals.Select(i => i.Length))
            .Max();

        #endregion

        #region Fields

        private static readonly string[] NotesPrettyNames = new string[Octave.OctaveSize];

        #endregion

        #region Methods

        public static NoteName[] GetChordNotesNames(NoteName rootNoteName, string chordCharacteristic, NoteName? bassNoteName)
        {
            var notesNames = new List<NoteName>();
            if (bassNoteName != null)
                notesNames.Add(bassNoteName.Value);

            var definition = NamesDefinitions.FirstOrDefault(d => d.Names.Contains(chordCharacteristic.Replace(" ", string.Empty)));
            if (definition != null)
                notesNames.AddRange(definition.Intervals.First().Select(i => rootNoteName.Transpose(Interval.FromHalfSteps(i))));

            return notesNames.ToArray();
        }

        public static IList<string> GetChordNames(NoteName[] notesNames)
        {
            var result = new List<string>();
            if (!notesNames.Any())
                return result;

            var notesNamesSet = new HashSet<NoteName>(notesNames).ToArray();

            result.AddRange(GetChordNamesInternal(notesNamesSet));

            var tail = new NoteName[notesNamesSet.Length - 1];

            for (var i = 0; i < notesNamesSet.Length; i++)
            {
                var k = 0;

                for (var j = 0; j < notesNamesSet.Length; j++)
                {
                    if (i == j)
                        continue;

                    tail[k++] = notesNamesSet[j];
                }

                var tailNames = GetChordNamesInternal(tail);
                result.AddRange(tailNames.Select(name => $"{name}/{GetPrettyName(notesNamesSet[i])}"));
            }

            return result.Distinct().OrderBy(n => n.Length).ToArray();
        }

        private static string GetPrettyName(NoteName noteName)
        {
            var index = (int)noteName;

            if (string.IsNullOrWhiteSpace(NotesPrettyNames[index]))
            {
                NotesPrettyNames[index] = noteName.ToString().Replace(Note.SharpLongString, Note.SharpShortString);
            }

            return NotesPrettyNames[index];
        }

        private static List<string> GetChordNamesInternal(ICollection<NoteName> notesNames)
        {
            var result = new List<string>();
            if (!notesNames.Any())
                return result;

            var notesNamesSet = new HashSet<NoteName>(notesNames);

            if (notesNamesSet.Count < MinIntervalsCount || notesNamesSet.Count > MaxIntervalsCount)
                return result;

            foreach (var noteName in notesNamesSet)
            {
                foreach (var nameDefinition in NamesDefinitions)
                {
                    foreach (var intervals in nameDefinition.Intervals)
                    {
                        if (notesNamesSet.Count != intervals.Length)
                            continue;

                        var checkFailed = false;

                        for (var i = 0; i < intervals.Length; i++)
                        {
                            var newNoteName = intervals[i] == 0
                                ? noteName
                                : (NoteName)(((int)noteName + intervals[i]) % Octave.OctaveSize);

                            if (!notesNamesSet.Contains(newNoteName))
                            {
                                checkFailed = true;
                                break;
                            }
                        }

                        if (checkFailed)
                            continue;

                        result.AddRange(nameDefinition.Names.Select(name => $"{GetPrettyName(noteName)}{name}"));
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
