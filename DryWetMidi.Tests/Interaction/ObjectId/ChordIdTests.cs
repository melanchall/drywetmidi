using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class ChordIdTests
    {
        #region Test methods

        [Test]
        public void CheckChordIdsEquality_1() => Assert.AreEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
            }),
            "Chord IDs equality check failed.");

        [Test]
        public void CheckChordIdsEquality_2() => Assert.AreEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
            }),
            "Chord IDs equality check failed.");

        [Test]
        public void CheckChordIdsEquality_3() => Assert.AreEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
            }),
            "Chord IDs equality check failed.");

        [Test]
        public void CheckChordIdsEquality_4() => Assert.AreEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)4, (SevenBitNumber)50),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)50),
            }),
            "Chord IDs equality check failed.");

        [Test]
        public void CheckChordIdsEquality_5() => Assert.AreEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)4, (SevenBitNumber)50),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)50),
            }),
            "Chord IDs equality check failed.");

        [Test]
        public void CheckChordIdsInequality_1() => Assert.AreNotEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
            }),
            "Chord IDs inequality check failed.");

        [Test]
        public void CheckChordIdsInequality_2() => Assert.AreNotEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)80),
            }),
            "Chord IDs inequality check failed.");

        [Test]
        public void CheckChordIdsInequality_3() => Assert.AreNotEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)50),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
            }),
            "Chord IDs inequality check failed.");

        [Test]
        public void CheckChordIdsInequality_4() => Assert.AreNotEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)3, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
            }),
            "Chord IDs inequality check failed.");

        [Test]
        public void CheckChordIdsInequality_5() => Assert.AreNotEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)80),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
            }),
            "Chord IDs inequality check failed.");

        [Test]
        public void CheckChordIdsInequality_6() => Assert.AreNotEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)4, (SevenBitNumber)50),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)60),
            }),
            "Chord IDs inequality check failed.");

        [Test]
        public void CheckChordIdsInequality_7() => Assert.AreNotEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)4, (SevenBitNumber)50),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)80),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)50),
            }),
            "Chord IDs inequality check failed.");

        [Test]
        public void CheckChordIdsInequality_8() => Assert.AreNotEqual(
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)4, (SevenBitNumber)50),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
            }),
            new ChordId(new[]
            {
                new NoteId((FourBitNumber)5, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)70),
                new NoteId((FourBitNumber)4, (SevenBitNumber)50),
            }),
            "Chord IDs inequality check failed.");

        #endregion
    }
}
