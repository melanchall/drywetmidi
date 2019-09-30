using System;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ChordUtilitiesTests
    {
        #region Test methods

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
    }
}
