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
            var isEqual = noteId.Equals(null);
            Assert.IsFalse(isEqual);
        }

        [Test]
        public void Equals_SameObject()
        {
            var noteId = new NoteId(new FourBitNumber(), new SevenBitNumber());
            var isEqual = noteId.Equals(noteId);
            Assert.IsTrue(isEqual);
        }

        [Test]
        public void Equals_DifferentObjectType()
        {
            var noteId = new NoteId(new FourBitNumber(), new SevenBitNumber());
            var obj = new object();
            var isEqual = noteId.Equals(obj);
            Assert.IsFalse(isEqual);
        }

        [Test]
        public void Equals_SameValues()
        {
            var noteId = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            var obj = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            var isEqual = noteId.Equals(obj);
            Assert.IsTrue(isEqual);
        }

        [Test]
        public void Equals_DifferentChannelValues()
        {
            var noteId = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            var obj = new NoteId(new FourBitNumber(2), new SevenBitNumber(2));
            var isEqual = noteId.Equals(obj);
            Assert.IsFalse(isEqual);
        }

        [Test]
        public void Equals_DifferentNoteNumberValues()
        {
            var noteId = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            var obj = new NoteId(new FourBitNumber(1), new SevenBitNumber(3));
            var isEqual = noteId.Equals(obj);
            Assert.IsFalse(isEqual);
        }

        #endregion

        #region GetHashCode

        [Test]
        public void GetHashCode_Empty()
        {
            var noteId1 = new NoteId(new FourBitNumber(), new SevenBitNumber());
            var noteId2 = new NoteId(new FourBitNumber(), new SevenBitNumber());
            Assert.AreEqual(noteId1.GetHashCode(), noteId2.GetHashCode(), "Hash codes must be consistent.");
        }

        [Test]
        public void GetHashCode_Same()
        {
            var noteId1 = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            var noteId2 = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            Assert.AreEqual(noteId1.GetHashCode(), noteId2.GetHashCode(), "Hash codes must be consistent.");
        }

        [Test]
        public void GetHashCode_Different()
        {
            var noteId1 = new NoteId(new FourBitNumber(1), new SevenBitNumber(2));
            var noteId2 = new NoteId(new FourBitNumber(2), new SevenBitNumber(1));
            Assert.AreNotEqual(noteId1.GetHashCode(), noteId2.GetHashCode(), "Hash codes must be consistent.");
        }

        #endregion

        #endregion
    }
}
