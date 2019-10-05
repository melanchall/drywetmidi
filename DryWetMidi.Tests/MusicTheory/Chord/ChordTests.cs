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

        #endregion
    }
}
