using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class NoteIdTests
    {
        #region Test methods

        #region Equals

        [Test]
        public void Equals_NullObject()
        {
            var noteId = new NoteId(new FourBitNumber(), new SevenBitNumber());
            Assert.AreNotEqual(noteId, null, "A null and non-null object are equal.");
        }

        [Test]
        public void Equals_SameObject()
        {
            var noteId = new NoteId(new FourBitNumber(), new SevenBitNumber());
            Assert.AreEqual(noteId, noteId, "An object is not equal to itself.");
        }

        [Test]
        public void Equals_DifferentObjectType()
        {
            var noteId1 = new NoteId(new FourBitNumber(), new SevenBitNumber());
            var noteId2 = new object();
            Assert.AreNotEqual(noteId1, noteId2, "Objects of different types are equal.");
        }

        [Test]
        public void Equals_SameValues()
        {
            var noteId1 = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            var noteId2 = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            Assert.AreEqual(noteId1, noteId2, "Identical objects are not equal.");
        }

        [Test]
        public void Equals_DifferentChannelValues()
        {
            var noteId1 = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            var noteId2 = new NoteId(new FourBitNumber(2), new SevenBitNumber(2));
            Assert.AreNotEqual(noteId1, noteId2, "Objects with mismatched channel values are equal.");
        }

        [Test]
        public void Equals_DifferentNoteNumberValues()
        {
            var noteId1 = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            var noteId2 = new NoteId(new FourBitNumber(1), new SevenBitNumber(3));
            Assert.AreNotEqual(noteId1, noteId2, "Objects with mismatched note number values are equal.");
        }

        #endregion

        #region GetHashCode

        [Test]
        public void GetHashCode_Empty()
        {
            var noteId1 = new NoteId(new FourBitNumber(), new SevenBitNumber());
            var noteId2 = new NoteId(new FourBitNumber(), new SevenBitNumber());
            Assert.AreEqual(noteId1.GetHashCode(), noteId2.GetHashCode(), "Hash codes for identical empty objects are different.");
        }

        [Test]
        public void GetHashCode_Same()
        {
            var noteId1 = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            var noteId2 = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            Assert.AreEqual(noteId1.GetHashCode(), noteId2.GetHashCode(), "Hash codes for identical objects are different.");
        }

        [Test]
        public void GetHashCode_Different()
        {
            var noteId1 = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            var noteId2 = new NoteId(new FourBitNumber(2), new SevenBitNumber(1));
            Assert.AreNotEqual(noteId1.GetHashCode(), noteId2.GetHashCode(), "Hash codes for different objects are the same.");
        }

        #endregion

        #endregion
    }
}
