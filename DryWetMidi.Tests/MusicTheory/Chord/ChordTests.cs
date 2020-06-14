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

        [TestCase("C", new[] { NoteName.C, NoteName.E, NoteName.G })]
        [TestCase("C6", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.A })]
        [TestCase("C7", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp })]
        [TestCase("Caug", new[] { NoteName.C, NoteName.E, NoteName.GSharp })]
        [TestCase("Caug7", new[] { NoteName.C, NoteName.E, NoteName.GSharp, NoteName.ASharp })]
        [TestCase("Cm", new[] { NoteName.C, NoteName.DSharp, NoteName.G })]
        [TestCase("Am", new[] { NoteName.A, NoteName.C, NoteName.E })]
        [TestCase("Cm", new[] { NoteName.C, NoteName.DSharp, NoteName.G })]
        [TestCase("Cm6", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.A })]
        [TestCase("Cm7", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.ASharp })]
        [TestCase("CmM7", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.B })]
        [TestCase("Cdim", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp })]
        [TestCase("Cdim7", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp, NoteName.A })]
        [TestCase("Cm7b5", new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp, NoteName.ASharp })]
        [TestCase("C5", new[] { NoteName.C, NoteName.G })]
        [TestCase("Csus4", new[] { NoteName.C, NoteName.F, NoteName.G })]
        [TestCase("Csus2", new[] { NoteName.C, NoteName.D, NoteName.G })]
        [TestCase("C9", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D })]
        [TestCase("C9sus4", new[] { NoteName.C, NoteName.F, NoteName.G, NoteName.ASharp, NoteName.D })]
        [TestCase("C9sus2", new[] { NoteName.C, NoteName.D, NoteName.G, NoteName.ASharp, NoteName.D })]
        [TestCase("F/G", new[] { NoteName.G, NoteName.F, NoteName.A, NoteName.C })]
        [TestCase("C11", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D, NoteName.F })]
        [TestCase("Cm11", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.ASharp, NoteName.D, NoteName.F })]
        [TestCase("C7b5", new[] { NoteName.C, NoteName.E, NoteName.FSharp, NoteName.ASharp })]
        [TestCase("Cadd9", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.D })]
        [TestCase("CmAdd9", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.D })]
        [TestCase("C6/9", new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.A, NoteName.D })]
        [TestCase("Cm6/9", new[] { NoteName.C, NoteName.DSharp, NoteName.G, NoteName.A, NoteName.D })]
        public void Parse(string input, NoteName[] expectedNotesNames)
        {
            var chord = Chord.Parse(input);
            CollectionAssert.AreEqual(expectedNotesNames, chord.NotesNames, "Notes names are invalid.");
        }

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
        [TestCase(new[] { NoteName.C, NoteName.D, NoteName.G, NoteName.ASharp, NoteName.D }, "C9sus2")]
        [TestCase(new[] { NoteName.G, NoteName.F, NoteName.A, NoteName.C }, "F/G")]
        // TODO
        // [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.F }, "Cadd11")]
        // [TestCase(new[] { NoteName.C, NoteName.E, NoteName.GSharp, NoteName.ASharp }, "C7#5")]
        // [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.CSharp }, "C7b9")]
        // [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.DSharp }, "C7#9")]
        // [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.ASharp, NoteName.D, NoteName.FSharp }, "C7#11")]
        // [TestCase(new[] { NoteName.C, NoteName.E, NoteName.G, NoteName.B, NoteName.D }, "C9")]
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

        #endregion
    }
}
