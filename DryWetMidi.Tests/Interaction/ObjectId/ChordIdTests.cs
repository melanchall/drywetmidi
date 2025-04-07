using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class ChordIdTests
    {
        #region Test methods

        [Test]
        public void CheckChordIdsEquality_1() => ClassicAssert.AreEqual(
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
        public void CheckChordIdsEquality_2() => ClassicAssert.AreEqual(
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
        public void CheckChordIdsEquality_3() => ClassicAssert.AreEqual(
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
        public void CheckChordIdsEquality_4() => ClassicAssert.AreEqual(
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
        public void CheckChordIdsEquality_5() => ClassicAssert.AreEqual(
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
        public void CheckChordIdsInequality_1() => ClassicAssert.AreNotEqual(
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
        public void CheckChordIdsInequality_2() => ClassicAssert.AreNotEqual(
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
        public void CheckChordIdsInequality_3() => ClassicAssert.AreNotEqual(
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
        public void CheckChordIdsInequality_4() => ClassicAssert.AreNotEqual(
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
        public void CheckChordIdsInequality_5() => ClassicAssert.AreNotEqual(
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
        public void CheckChordIdsInequality_6() => ClassicAssert.AreNotEqual(
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
        public void CheckChordIdsInequality_7() => ClassicAssert.AreNotEqual(
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
        public void CheckChordIdsInequality_8() => ClassicAssert.AreNotEqual(
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
