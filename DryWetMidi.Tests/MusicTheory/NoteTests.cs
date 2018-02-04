using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestClass]
    public class NoteTests
    {
        #region Test methods

        [TestMethod]
        [Description("Check that notes of the same note number are equal by reference.")]
        public void CheckReferences()
        {
            Assert.IsTrue(ReferenceEquals(Note.Get((SevenBitNumber)34), Note.Get((SevenBitNumber)34)));
        }

        [TestMethod]
        [Description("Transpose a note up.")]
        public void Transpose_Up()
        {
            var expectedNote = Note.Get((SevenBitNumber)25);
            var actualNote = Note.Get((SevenBitNumber)15)
                                 .Transpose(Interval.FromHalfSteps(10));

            Assert.AreEqual(expectedNote, actualNote);
        }

        [TestMethod]
        [Description("Transpose a note up by maximum value.")]
        public void Transpose_Up_Max()
        {
            var expectedNote = Note.Get(SevenBitNumber.MaxValue);
            var actualNote = Note.Get(SevenBitNumber.MinValue)
                                 .Transpose(Interval.GetUp(SevenBitNumber.MaxValue));

            Assert.AreEqual(expectedNote, actualNote);
        }

        [TestMethod]
        [Description("Transpose a note up going out of the valid range.")]
        public void Transpose_Up_OutOfRange()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                Note.Get(SevenBitNumber.MaxValue)
                    .Transpose(Interval.GetUp(SevenBitNumber.MaxValue));
            });
        }

        [TestMethod]
        [Description("Transpose a note down.")]
        public void Transpose_Down()
        {
            var expectedNote = Note.Get((SevenBitNumber)25);
            var actualNote = Note.Get((SevenBitNumber)35)
                                 .Transpose(Interval.FromHalfSteps(-10));

            Assert.AreEqual(expectedNote, actualNote);
        }

        [TestMethod]
        [Description("Transpose a note down by maximum value.")]
        public void Transpose_Down_Max()
        {
            var expectedNote = Note.Get(SevenBitNumber.MinValue);
            var actualNote = Note.Get(SevenBitNumber.MaxValue)
                                 .Transpose(Interval.GetDown(SevenBitNumber.MaxValue));

            Assert.AreEqual(expectedNote, actualNote);
        }

        [TestMethod]
        [Description("Transpose a note down going out of the valid range.")]
        public void Transpose_Down_OutOfRange()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                Note.Get(SevenBitNumber.MinValue)
                    .Transpose(Interval.GetDown(SevenBitNumber.MaxValue));
            });
        }

        #endregion
    }
}
