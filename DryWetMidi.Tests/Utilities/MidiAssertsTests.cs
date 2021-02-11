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
                "Notes aren't equal."));
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Channel()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Channel = (FourBitNumber)7 } },
                "Notes aren't equal."));
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Velocity()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)7 } },
                "Notes aren't equal."));
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_OffVelocity()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { OffVelocity = (SevenBitNumber)4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { OffVelocity = (SevenBitNumber)7 } },
                "Notes aren't equal."));
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Time()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Time = 4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Time = 7 } },
                "Notes aren't equal."));
        }

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Length()
        {
            Assert.Throws<AssertionException>(() => MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Length = 4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Length = 7 } },
                "Notes aren't equal."));
        }

        #endregion
    }
}
