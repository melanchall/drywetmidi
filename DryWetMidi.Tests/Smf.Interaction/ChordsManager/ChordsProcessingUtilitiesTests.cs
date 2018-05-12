using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public sealed class ChordsProcessingUtilitiesTests
    {
        #region Constants

        private static readonly Func<Chord> TwoNotesChordCreator = () => new Chord(new Note((SevenBitNumber)100, 200, 100),
                                                                                   new Note((SevenBitNumber)110, 300, 130));

        #endregion

        #region Test methods

        [Test]
        [Description("Split null chord.")]
        public void Split_Null()
        {
            var chord = default(Chord);
            var time = 100;

            Assert.Throws<ArgumentNullException>(() => chord.Split(time));
        }

        [Test]
        [Description("Split empty chord.")]
        public void Split_Empty()
        {
            Func<Chord> chordCreator = () => new Chord();

            var chord = chordCreator();
            var time = 100;

            var parts = chord.Split(time);
            Assert.IsNull(parts.Item2,
                          "Right part is not null.");
            Assert.AreNotSame(parts.Item1,
                              chord,
                              "Left part refers to the same object as the original chord.");
            Assert.IsTrue(ChordEquality.AreEqual(chordCreator(), parts.Item1),
                          "Left part doesn't equal to the original chord.");
        }

        [Test]
        [Description("Split a chord of zero length.")]
        public void Split_ZeroLength()
        {
            Func<Chord> chordCreator = () => new Chord(new Note((SevenBitNumber)100));

            var chord = chordCreator();
            var time = 0;

            var parts = chord.Split(time);
            Assert.IsNull(parts.Item1,
                          "Left part is not null.");
            Assert.AreNotSame(parts.Item2,
                              chord,
                              "Right part refers to the same object as the original chord.");
            Assert.IsTrue(ChordEquality.AreEqual(chordCreator(), parts.Item2),
                          "Right part doesn't equal to the original chord.");
        }

        [Test]
        [Description("Split by time below the start time of a chord.")]
        public void Split_TimeBelowStartTime()
        {
            var chordCreator = TwoNotesChordCreator;

            var chord = chordCreator();
            var time = 50;

            var parts = chord.Split(time);
            Assert.IsNull(parts.Item1,
                          "Left part is not null.");
            Assert.AreNotSame(parts.Item2,
                              chord,
                              "Right part refers to the same object as the original chord.");
            Assert.IsTrue(ChordEquality.AreEqual(chordCreator(), parts.Item2),
                          "Right part doesn't equal to the original chord.");
        }

        [Test]
        [Description("Split by time above the end time of a chord.")]
        public void Split_TimeAboveEndTime()
        {
            Func<Chord> chordCreator = TwoNotesChordCreator;

            var chord = chordCreator();
            var time = 500;

            var parts = chord.Split(time);
            Assert.IsNull(parts.Item2,
                          "Right part is not null.");
            Assert.AreNotSame(parts.Item1,
                              chord,
                              "Left part refers to the same object as the original chord.");
            Assert.IsTrue(ChordEquality.AreEqual(chordCreator(), parts.Item1),
                          "Left part doesn't equal to the original chord.");
        }

        [Test]
        [Description("Split a chord by time falling inside it and intersecting all notes of the chord.")]
        public void Split_TimeInsideChord_IntersectingAllNotes()
        {
            Func<Chord> chordCreator = TwoNotesChordCreator;

            var note = chordCreator();
            var time = 150;

            var parts = note.Split(time);
            var expectedLeftChord = new Chord(new Note((SevenBitNumber)100, 50, 100),
                                              new Note((SevenBitNumber)110, 20, 130));
            var expectedRightChord = new Chord(new Note((SevenBitNumber)100, 150, 150),
                                               new Note((SevenBitNumber)110, 280, 150));

            Assert.IsTrue(ChordEquality.AreEqual(expectedLeftChord, parts.Item1),
                          "Left part is invalid.");
            Assert.IsTrue(ChordEquality.AreEqual(expectedRightChord, parts.Item2),
                          "Right part is invalid.");
        }

        [Test]
        [Description("Split a chord by time falling inside it and intersecting not all notes of the chord.")]
        public void Split_TimeInsideChord_IntersectingNotAllNotes()
        {
            Func<Chord> chordCreator = TwoNotesChordCreator;

            var chord = chordCreator();
            var time = 120;

            var parts = chord.Split(time);
            var expectedLeftChord = new Chord(new Note((SevenBitNumber)100, 20, 100));
            var expectedRightChord = new Chord(new Note((SevenBitNumber)100, 180, 120),
                                               new Note((SevenBitNumber)110, 300, 130));

            Assert.IsTrue(ChordEquality.AreEqual(expectedLeftChord, parts.Item1),
                          "Left part is invalid.");
            Assert.IsTrue(ChordEquality.AreEqual(expectedRightChord, parts.Item2),
                          "Right part is invalid.");
        }

        #endregion
    }
}
