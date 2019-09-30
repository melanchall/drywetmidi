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

        #endregion
    }
}
