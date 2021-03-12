using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    [TestFixture]
    public sealed class MidiAssertsTests
    {
        #region Test methods

        [Test]
        public void AreEqual_NotesCollections_Equal()
        {
            MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) },
                new[] { new Note(SevenBitNumber.MaxValue) },
                "Notes aren't equal.");
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_NoteNumber()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) },
                new[] { new Note(SevenBitNumber.MinValue) },
                "Notes are equal."));
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Channel()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Channel = (FourBitNumber)7 } },
                "Notes are equal."));
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Velocity()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)7 } },
                "Notes are equal."));
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_OffVelocity()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { OffVelocity = (SevenBitNumber)4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { OffVelocity = (SevenBitNumber)7 } },
                "Notes are equal."));
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Time()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Time = 4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Time = 7 } },
                "Notes are equal."));
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Length()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Length = 4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Length = 7 } },
                "Notes are equal."));
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Order()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue), new Note(SevenBitNumber.MinValue) },
                new[] { new Note(SevenBitNumber.MinValue), new Note(SevenBitNumber.MaxValue) },
                "Notes are equal."));
        }

        [Test]
        public void AreEqual_ChordsCollections_Equal()
        {
            MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue)) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue)) },
                "Chords aren't equal.");
        }

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_NoteNumber()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue)) },
                new[] { new Chord(new Note(SevenBitNumber.MinValue)) },
                "Chords are equal."));
        }

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_Channel()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 }) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Channel = (FourBitNumber)7 }) },
                "Chords are equal."));
        }

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_Velocity()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)4 }) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)7 }) },
                "Chords are equal."));
        }

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_OffVelocity()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { OffVelocity = (SevenBitNumber)4 }) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { OffVelocity = (SevenBitNumber)7 }) },
                "Chords are equal."));
        }

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_Time()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Time = 4 }) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Time = 7 }) },
                "Chords are equal."));
        }

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_Length()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Length = 4 }) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Length = 7 }) },
                "Chords are equal."));
        }

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_NotesCount()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue), new Note(SevenBitNumber.MinValue)) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue)) },
                "Chords are equal."));
        }

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_Order()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue)), new Chord(new Note(SevenBitNumber.MinValue)) },
                new[] { new Chord(new Note(SevenBitNumber.MinValue)), new Chord(new Note(SevenBitNumber.MaxValue)) },
                "Chords are equal."));
        }

        #endregion
    }
}
