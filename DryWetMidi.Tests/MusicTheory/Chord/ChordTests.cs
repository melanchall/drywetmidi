using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ChordTests
    {
        private static readonly object[] ParseData_Valid = new[]
        {
            new object[] { "C", new[] { NoteName.C, NoteName.E, NoteName.G } },
            new object[] { "  C", new[] { NoteName.C, NoteName.E, NoteName.G } },
            new object[] { "C6", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.A } },
            new object[] { "C7", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp } },
            new object[] { "Caug", new[] { NoteName.C, NoteName.E, NoteName.GSharp } },
            new object[] { "Caug7", new[] { NoteName.C, NoteName.E, NoteName.GSharp, NoteName.ASharp } },
            new object[] { "Cm", new[] { NoteName.C, NoteName.DSharp, NoteName.G } },
            new object[] { "Am", new[] { NoteName.A, NoteName.C, NoteName.E } },
            new object[] { "Cm", new[] { NoteName.C, NoteName.DSharp, NoteName.G } },
            new object[] { "C m6", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.A } },
            new object[] { "Cm7", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.ASharp } },
            new object[] { "CmM7", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.B } },
            new object[] { "Cdim", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp } },
            new object[] { "Cdim7", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp, NoteName.A } },
            new object[] { "Cm7b5", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp, NoteName.ASharp } },
            new object[] { "C5", new[] { NoteName.C, NoteName.G } },
            new object[] { "Csus4", new[] { NoteName.C, NoteName.F, NoteName.G } },
            new object[] { "Csus2", new[] { NoteName.C, NoteName.D, NoteName.G } },
            new object[] { "C9", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D } },
            new object[] { "C9sus4", new[] { NoteName.C, NoteName.F, NoteName.G, NoteName.ASharp, NoteName.D } },
            new object[] { "F/G", new[] { NoteName.G, NoteName.F, NoteName.A, NoteName.C } },
            new object[] { "F / G", new[] { NoteName.G, NoteName.F, NoteName.A, NoteName.C } },
            new object[] { "C11", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D, NoteName.F } },
            new object[] { "Cm11", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.ASharp, NoteName.D, NoteName.F } },
            new object[] { "C7b5", new[] { NoteName.C, NoteName.E, NoteName.FSharp, NoteName.ASharp } },
            new object[] { "Cadd9", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.D } },
            new object[] { "CmAdd9", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.D } },
            new object[] { "C6/9", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.A, NoteName.D } },
            new object[] { "Cm6/9", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.A, NoteName.D } },
        };

        private static readonly object[] ParseData_Invalid = new[]
        {
            new object[] { "X" },
            new object[] { "Cc" },
            new object[] { "C66" },
            new object[] { "aC7" },
            new object[] { "Caugg" },
            new object[] { "C_aug_7" },
            new object[] { "C-m" },
            new object[] { "aam" },
            new object[] { "cC" },
            new object[] { "something" },
            new object[] { "87" },
            new object[] { "M7" },
        };

        #region Test methods

        [Test]
        public void CreateWithEmptyNotesCollection()
        {
            ClassicAssert.Throws<ArgumentException>(() => new Chord(Enumerable.Empty<NoteName>().ToArray()));
        }

        [Test]
        public void CreateWithInvalidNotes()
        {
            ClassicAssert.Throws<InvalidEnumArgumentException>(() => new Chord((NoteName)100, new NoteName[0]));
        }

        [Test]
        public void CreateWithValidNotes()
        {
            var chord = new Chord(NoteName.A, NoteName.B);
            CollectionAssert.AreEqual(new[] { NoteName.A, NoteName.B }, chord.NotesNames, "Notes names are invalid.");
            ClassicAssert.AreEqual(NoteName.A, chord.RootNoteName, "Root note name is invalid.");
        }

        [Test]
        public void CreateByIntervals()
        {
            var chord = new Chord(NoteName.A, Interval.FromHalfSteps(2), Interval.FromHalfSteps(5));
            CollectionAssert.AreEqual(new[] { NoteName.A, NoteName.B, NoteName.D }, chord.NotesNames, "Notes names are invalid.");
            ClassicAssert.AreEqual(NoteName.A, chord.RootNoteName, "Root note name is invalid.");
        }

        [Test]
        public void CreateByIntervals_Negative()
        {
            var chord = new Chord(NoteName.A, Interval.FromHalfSteps(2), Interval.FromHalfSteps(-1), Interval.FromHalfSteps(5));
            CollectionAssert.AreEqual(new[] { NoteName.GSharp, NoteName.A, NoteName.B, NoteName.D }, chord.NotesNames, "Notes names are invalid.");
            ClassicAssert.AreEqual(NoteName.GSharp, chord.RootNoteName, "Root note name is invalid.");
        }

        [Test]
        public void CheckEquality()
        {
            var chord1 = new Chord(NoteName.A, NoteName.B);
            var chord2 = new Chord(NoteName.A, NoteName.B);
            var chord3 = new Chord(NoteName.B, NoteName.A);

            ClassicAssert.AreEqual(chord1, chord2, "Chords are not equal.");
            ClassicAssert.AreNotEqual(chord1, chord3, "Chords are equal.");
        }

        [TestCase(NoteName.C, ChordQuality.Major, new[] { NoteName.C, NoteName.E, NoteName.G })]
        [TestCase(NoteName.F, ChordQuality.Major, new[] { NoteName.F, NoteName.A, NoteName.C })]
        [TestCase(NoteName.C, ChordQuality.Minor, new[] { NoteName.C, NoteName.DSharp, NoteName.G })]
        [TestCase(NoteName.F, ChordQuality.Minor, new[] { NoteName.F, NoteName.GSharp, NoteName.C })]
        [TestCase(NoteName.C, ChordQuality.Augmented, new[] { NoteName.C, NoteName.E, NoteName.GSharp })]
        [TestCase(NoteName.F, ChordQuality.Augmented, new[] { NoteName.F, NoteName.A, NoteName.CSharp })]
        [TestCase(NoteName.C, ChordQuality.Diminished, new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp })]
        [TestCase(NoteName.F, ChordQuality.Diminished, new[] { NoteName.F, NoteName.GSharp, NoteName.B })]
        public void GetByTriad(NoteName rootNoteName, ChordQuality chordQuality, NoteName[] expectedNotesNames)
        {
            var chord = Chord.GetByTriad(rootNoteName, chordQuality);
            CollectionAssert.AreEqual(expectedNotesNames, chord.NotesNames, "Notes names are invalid.");
        }

        [TestCase(NoteName.C, ChordQuality.Major, new[] { "P8" }, new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.C })]
        [TestCase(NoteName.C, ChordQuality.Major, new[] { "-1", "-3" }, new[] { NoteName.A, NoteName.B, NoteName.C, NoteName.E, NoteName.G })]
        public void GetByTriad_AdditionalIntervals(NoteName rootNoteName, ChordQuality chordQuality, string[] intervals, NoteName[] expectedNotesNames)
        {
            var chord = Chord.GetByTriad(rootNoteName, chordQuality, intervals.Select(i => Interval.Parse(i)).ToArray());
            CollectionAssert.AreEqual(expectedNotesNames, chord.NotesNames, "Notes names are invalid.");
        }

        [TestCaseSource(nameof(ParseData_Valid))]
        public void Parse_Valid(string input, NoteName[] expectedNotesNames)
        {
            var chord = Chord.Parse(input);
            CollectionAssert.AreEqual(expectedNotesNames, chord.NotesNames, "Notes names are invalid.");
        }

        [TestCaseSource(nameof(ParseData_Valid))]
        public void TryParse_Valid(string input, NoteName[] expectedNotesNames)
        {
            ClassicAssert.IsTrue(Chord.TryParse(input, out var chord), "Chord parsing failed.");
            CollectionAssert.AreEqual(expectedNotesNames, chord.NotesNames, "Notes names are invalid.");
        }

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void Parse_Invalid(string input) =>
            ClassicAssert.Throws<FormatException>(() => Chord.Parse(input), "Chord parsing did not throw an exception.");

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void TryParse_Invalid(string input) =>
            ClassicAssert.IsFalse(Chord.TryParse(input, out var chord), "Chord parsing succeeded.");

        [Test]
        public void Parse_Invalid_EmptyOrNull([Values(null, "", "  ")] string input) =>
            ClassicAssert.Throws<ArgumentException>(() => Chord.Parse(input), "Chord parsing did not throw an exception.");

        [Test]
        public void TryParse_Invalid_EmptyOrNull([Values(null, "", "  ")] string input) =>
            ClassicAssert.IsFalse(Chord.TryParse(input, out var chord), $"Parsed invalid value '{input}'.");

        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G }, "C")]
        [TestCase(new[] { NoteName.C, NoteName.G, NoteName.E }, "C")]
        [TestCase(new[] { NoteName.E, NoteName.G, NoteName.C }, "C")]
        [TestCase(new[] { NoteName.E, NoteName.C, NoteName.G }, "C")]
        [TestCase(new[] { NoteName.G, NoteName.E, NoteName.C }, "C")]
        [TestCase(new[] { NoteName.G, NoteName.C, NoteName.E }, "C")]
        [TestCase(new[] { NoteName.GSharp, NoteName.C, NoteName.E, NoteName.G }, "C/G#")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.C, NoteName.E }, "C")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.A }, "C6")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp }, "C7")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.GSharp }, "Caug")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.GSharp, NoteName.ASharp }, "Caug7")]
        [TestCase(new[] { NoteName.C, NoteName.DSharp, NoteName.G }, "Cm")]
        [TestCase(new[] { NoteName.A, NoteName.C, NoteName.E }, "Am")]
        [TestCase(new[] { NoteName.C, NoteName.DSharp, NoteName.G }, "Cm")]
        [TestCase(new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.A }, "Cm6")]
        [TestCase(new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.ASharp }, "Cm7")]
        [TestCase(new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.B }, "CmM7")]
        [TestCase(new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp }, "Cdim")]
        [TestCase(new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp, NoteName.A }, "Cdim7")]
        [TestCase(new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp, NoteName.ASharp }, "Cm7b5")]
        [TestCase(new[] { NoteName.C, NoteName.G }, "C5")]
        [TestCase(new[] { NoteName.C, NoteName.G, NoteName.C }, "C5")]
        [TestCase(new[] { NoteName.C, NoteName.F, NoteName.G }, "Csus4")]
        [TestCase(new[] { NoteName.C, NoteName.D, NoteName.G }, "Csus2")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D }, "C9")]
        [TestCase(new[] { NoteName.C, NoteName.F, NoteName.G, NoteName.ASharp, NoteName.D }, "C9sus4")]
        [TestCase(new[] { NoteName.G, NoteName.F, NoteName.A, NoteName.C }, "F/G")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.F }, "Cadd11")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.GSharp, NoteName.ASharp }, "C7#5")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.CSharp }, "C7b9")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.DSharp }, "C7#9")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.FSharp }, "C7#11")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D }, "C9")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D, NoteName.F }, "C11")]
        [TestCase(new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.ASharp, NoteName.D, NoteName.F }, "Cm11")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.FSharp, NoteName.ASharp }, "C7b5")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.D }, "Cadd9")]
        [TestCase(new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.D }, "CmAdd9")]
        [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.A, NoteName.D }, "C6/9")]
        [TestCase(new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.A, NoteName.D }, "Cm6/9")]
        public void GetNames(NoteName[] notesNames, string expectedChordName)
        {
            var chord = new Chord(notesNames);
            var names = chord.GetNames();
            CollectionAssert.Contains(names, expectedChordName);
        }

        [Test]
        public void CheckChordNamesTableDoesntContainOctaves()
        {
            var namesWithOctaves = new HashSet<string>();

            foreach (var nameDefinition in ChordsNamesTable.NamesDefinitions)
            {
                foreach (var intervals in nameDefinition.Intervals)
                {
                    for (var i = 0; i < intervals.Length; i++)
                    {
                        for (var j = i + 1; j < intervals.Length; j++)
                        {
                            var delta = Math.Abs(intervals[i] - intervals[j]);
                            if (delta % 12 == 0)
                            {
                                namesWithOctaves.Add(string.Join(", ", nameDefinition.Names));
                            }
                        }
                    }
                }
            }

            CollectionAssert.IsEmpty(namesWithOctaves, "There are names with octaves.");
        }

        [Test]
        public void CheckChordNamesTableDoesntContainDuplicatedIntervalsSets()
        {
            var intervalsStrings = new HashSet<string>();
            var duplicatedIntervalsStrings = new HashSet<string>();

            foreach (var nameDefinition in ChordsNamesTable.NamesDefinitions)
            {
                foreach (var intervals in nameDefinition.Intervals)
                {
                    var intervalsString = string.Join(" ", intervals);
                    if (!intervalsStrings.Add(intervalsString))
                        duplicatedIntervalsStrings.Add(intervalsString);
                }
            }

            CollectionAssert.IsEmpty(duplicatedIntervalsStrings, "There are duplicated intervals sets.");
        }

        #endregion
    }
}
