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
            Assert.Throws<ArgumentException>(() => new Chord(Enumerable.Empty<NoteName>()));
        }

        [Test]
        public void CreateWithInvalidNotes()
        {
            Assert.Throws<InvalidEnumArgumentException>(() => new Chord((NoteName)100));
        }

        [Test]
        public void CreateWithValidNotes()
        {
            var chord = new Chord(NoteName.A, NoteName.B);
            CollectionAssert.AreEqual(new[] { NoteName.A, NoteName.B }, chord.NotesNames, "Notes names are invalid.");
            Assert.AreEqual(NoteName.A, chord.RootNoteName, "Root note name is invalid.");
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
