using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ChordUtilitiesTests
    {
        #region Test methods

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
            var inversions = chord.GetInversions().Select(c => c.NotesNames).ToArray();
            Assert.AreEqual(4, inversions.Length, "Invalid count of inversions.");
            AssertCollectionContainsCollection(
                inversions,
                new[] { NoteName.E, NoteName.G, NoteName.C },
                "First inversion (E G C) is invalid.");
            AssertCollectionContainsCollection(
                inversions,
                new[] { NoteName.E, NoteName.C, NoteName.G },
                "First inversion (E C G) is invalid.");
            AssertCollectionContainsCollection(
                inversions,
                new[] { NoteName.G, NoteName.C, NoteName.E },
                "Second inversion (G C E) is invalid.");
            AssertCollectionContainsCollection(
                inversions,
                new[] { NoteName.G, NoteName.E, NoteName.C },
                "Second inversion (G E C) is invalid.");
        }

        [Test]
        public void ResolveRootNote()
        {
            var chord = new Chord(NoteName.A, NoteName.ASharp, NoteName.D);
            var rootNote = chord.ResolveRootNote(Octave.Get(4));
            Assert.AreEqual(Notes.A4, rootNote, "Resolved root note is invalid.");
        }

        [Test]
        public void GetIntervalsFromRootNote()
        {
            var chord = new Chord(NoteName.A, NoteName.ASharp, NoteName.D, NoteName.D);
            var intervals = chord.GetIntervalsFromRootNote();
            CollectionAssert.AreEqual(
                new[] { Interval.FromHalfSteps(1), Interval.FromHalfSteps(5), Interval.FromHalfSteps(17) },
                intervals,
                "Intervals are invalid.");
        }

        [Test]
        public void GetIntervalsFromRootNote_OutOfRange()
        {
            var chord = new Chord(NoteName.A, NoteName.A, NoteName.A, NoteName.A, NoteName.A, NoteName.A, NoteName.A, NoteName.A, NoteName.A, NoteName.A, NoteName.A, NoteName.A);
            Assert.Throws<InvalidOperationException>(() => chord.GetIntervalsFromRootNote());
        }

        [Test]
        public void GetIntervalsBetweenNotes()
        {
            var chord = new Chord(NoteName.A, NoteName.ASharp, NoteName.D, NoteName.D);
            var intervals = chord.GetIntervalsBetweenNotes();
            CollectionAssert.AreEqual(
                new[] { Interval.FromHalfSteps(1), Interval.FromHalfSteps(4), Interval.FromHalfSteps(12) },
                intervals,
                "Intervals are invalid.");
        }

        [Test]
        public void ResolveNotes()
        {
            var chord = new Chord(NoteName.A, NoteName.ASharp, NoteName.D);
            var notes = chord.ResolveNotes(Octave.Get(2));
            CollectionAssert.AreEqual(
                new[] { Notes.A2, Notes.ASharp2, Notes.D3 },
                notes,
                "Resolved notes are invalid.");
        }

        #endregion

        #region Private methods

        private void AssertCollectionContainsCollection<T>(ICollection<ICollection<T>> collection, ICollection<T> target, string errorMessage)
        {
            foreach (var element in collection)
            {
                if (element.SequenceEqual(target))
                    return;
            }

            Assert.Fail(errorMessage);
        }

        #endregion
    }
}
