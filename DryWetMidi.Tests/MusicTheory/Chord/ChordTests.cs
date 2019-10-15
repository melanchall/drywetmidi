using System;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ChordTests
    {
        #region Test methods

        [Test]
        public void CreateWithEmptyNotesCollection()
        {
            Assert.Throws<ArgumentException>(() => new Chord(Enumerable.Empty<NoteName>().ToArray()));
        }

        [Test]
        public void CreateWithInvalidNotes()
        {
            Assert.Throws<InvalidEnumArgumentException>(() => new Chord((NoteName)100, new NoteName[0]));
        }

        [Test]
        public void CreateWithValidNotes()
        {
            var chord = new Chord(NoteName.A, NoteName.B);
            CollectionAssert.AreEqual(new[] { NoteName.A, NoteName.B }, chord.NotesNames, "Notes names are invalid.");
            Assert.AreEqual(NoteName.A, chord.RootNoteName, "Root note name is invalid.");
        }

        [Test]
        public void CreateByIntervals()
        {
            var chord = new Chord(NoteName.A, Interval.FromHalfSteps(2), Interval.FromHalfSteps(5));
            CollectionAssert.AreEqual(new[] { NoteName.A, NoteName.B, NoteName.D }, chord.NotesNames, "Notes names are invalid.");
            Assert.AreEqual(NoteName.A, chord.RootNoteName, "Root note name is invalid.");
        }

        [Test]
        public void CreateByIntervals_Negative()
        {
            var chord = new Chord(NoteName.A, Interval.FromHalfSteps(2), Interval.FromHalfSteps(-1), Interval.FromHalfSteps(5));
            CollectionAssert.AreEqual(new[] { NoteName.GSharp, NoteName.A, NoteName.B, NoteName.D }, chord.NotesNames, "Notes names are invalid.");
            Assert.AreEqual(NoteName.GSharp, chord.RootNoteName, "Root note name is invalid.");
        }

        [Test]
        public void CheckEquality()
        {
            var chord1 = new Chord(NoteName.A, NoteName.B);
            var chord2 = new Chord(NoteName.A, NoteName.B);
            var chord3 = new Chord(NoteName.B, NoteName.A);

            Assert.AreEqual(chord1, chord2, "Chords are not equal.");
            Assert.AreNotEqual(chord1, chord3, "Chords are equal.");
        }

        [Test]
        public void GetInversions_OneNote()
        {
            var chord = new Chord(NoteName.C, new NoteName[0]);
            var inversions = chord.GetInversions();
            CollectionAssert.IsEmpty(inversions, "There are inversions for one-note chord.");
        }

        [Test]
        public void GetInversions()
        {
            var chord = new Chord(NoteName.C, NoteName.E, NoteName.G);
            var inversions = chord.GetInversions().ToArray();
            Assert.AreEqual(2, inversions.Length, "Invalid count of inversions.");
            CollectionAssert.AreEqual(
                new[] { NoteName.E, NoteName.G, NoteName.C },
                inversions[0].NotesNames,
                "First inversion is invalid.");
            CollectionAssert.AreEqual(
                new[] { NoteName.G, NoteName.C, NoteName.E },
                inversions[1].NotesNames,
                "Second inversion is invalid.");
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

        [TestCase("C M3 P5", new[] { NoteName.C, NoteName.E, NoteName.G })]
        [TestCase("C 4 7", new[] { NoteName.C, NoteName.E, NoteName.G })]
        [TestCase("C 4 7 12", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.C })]
        [TestCase("C 4 7 -1 12", new[] { NoteName.B, NoteName.C, NoteName.E, NoteName.G, NoteName.C })]
        [TestCase("F M3 P5", new[] { NoteName.F, NoteName.A, NoteName.C })]
        [TestCase("C m3 P5", new[] { NoteName.C, NoteName.DSharp, NoteName.G })]
        [TestCase("F m3 P5", new[] { NoteName.F, NoteName.GSharp, NoteName.C })]
        [TestCase("C M3 A5", new[] { NoteName.C, NoteName.E, NoteName.GSharp })]
        [TestCase("F M3 A5", new[] { NoteName.F, NoteName.A, NoteName.CSharp })]
        [TestCase("C m3 d5", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp })]
        [TestCase("F m3 d5", new[] { NoteName.F, NoteName.GSharp, NoteName.B })]
        [TestCase("C", new[] { NoteName.C, NoteName.E, NoteName.G })]
        [TestCase("C6", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.A })]
        [TestCase("CM6", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.A })]
        [TestCase("Cmaj6", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.A })]
        [TestCase("C7", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp })]
        [TestCase("Cdom7", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp })]
        [TestCase("CM7", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.B })]
        [TestCase("Cmaj7", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.B })]
        [TestCase("C+", new[] { NoteName.C, NoteName.E, NoteName.GSharp })]
        [TestCase("Caug", new[] { NoteName.C, NoteName.E, NoteName.GSharp })]
        [TestCase("C+7", new[] { NoteName.C, NoteName.E, NoteName.GSharp, NoteName.ASharp })]
        [TestCase("Caug7", new[] { NoteName.C, NoteName.E, NoteName.GSharp, NoteName.ASharp })]
        [TestCase("Cm", new[] { NoteName.C, NoteName.DSharp, NoteName.G })]
        [TestCase("Am", new[] { NoteName.A, NoteName.C, NoteName.E })]
        [TestCase("Cmin", new[] { NoteName.C, NoteName.DSharp, NoteName.G })]
        [TestCase("Cm6", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.A })]
        [TestCase("Cmin6", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.A })]
        [TestCase("Cm7", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.ASharp })]
        [TestCase("Cmin7", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.ASharp })]
        [TestCase("CmM7", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.B })]
        [TestCase("Cm/M7", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.B })]
        [TestCase("Cm(M7)", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.B })]
        [TestCase("Cminmaj7", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.B })]
        [TestCase("Cmin/maj7", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.B })]
        [TestCase("Cmin(maj7)", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.B })]
        [TestCase("Cdim", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp })]
        [TestCase("Cdim7", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp, NoteName.A })]
        [TestCase("Cø", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp, NoteName.ASharp })]
        [TestCase("Cø7", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp, NoteName.ASharp })]
        [TestCase("Cm7b5", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp, NoteName.ASharp })]
        [TestCase("C5", new[] { NoteName.C, NoteName.G})]
        [TestCase("Csus4", new[] { NoteName.C, NoteName.F, NoteName.G })]
        [TestCase("Csus2", new[] { NoteName.C, NoteName.D, NoteName.G })]
        [TestCase("C9sus4", new[] { NoteName.C, NoteName.F, NoteName.G, NoteName.ASharp, NoteName.D })]
        [TestCase("F/G", new[] { NoteName.G, NoteName.F, NoteName.A, NoteName.C })]
        [TestCase("Cadd11", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.F })]
        [TestCase("C7+5", new[] { NoteName.C, NoteName.E, NoteName.GSharp, NoteName.ASharp })]
        [TestCase("C7#5", new[] { NoteName.C, NoteName.E, NoteName.GSharp, NoteName.ASharp })]
        [TestCase("C7-9", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.CSharp })]
        [TestCase("C7b9", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.CSharp })]
        [TestCase("C7+9", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.DSharp })]
        [TestCase("C7#9", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.DSharp })]
        [TestCase("C7+11", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D, NoteName.FSharp })]
        [TestCase("C7#11", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D, NoteName.FSharp })]
        [TestCase("C7-13", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D, NoteName.F, NoteName.GSharp })]
        [TestCase("C7b13", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D, NoteName.F, NoteName.GSharp })]
        public void Parse(string input, NoteName[] expectedNotesNames)
        {
            var chord = Chord.Parse(input);
            CollectionAssert.AreEqual(expectedNotesNames, chord.NotesNames, "Notes names are invalid.");
        }

        #endregion
    }
}
