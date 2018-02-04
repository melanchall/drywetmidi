using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestClass]
    public class NoteDefinitionTests
    {
        #region Test methods

        [TestMethod]
        [Description("Check that notes definitions of the same note number are equal by reference.")]
        public void CheckReferences()
        {
            Assert.IsTrue(ReferenceEquals(Note.Get((SevenBitNumber)34), Note.Get((SevenBitNumber)34)));
        }

        [TestMethod]
        [Description("Transpose note definition up.")]
        public void Transpose_Up()
        {
            var expectedNoteDefinition = Note.Get((SevenBitNumber)25);
            var actualNoteDefinition = Note.Get((SevenBitNumber)15)
                                                     .Transpose(Interval.FromHalfSteps(10));

            Assert.AreEqual(expectedNoteDefinition, actualNoteDefinition);
        }

        [TestMethod]
        [Description("Transpose note definition up by maximum value.")]
        public void Transpose_Up_Max()
        {
            var expectedNoteDefinition = Note.Get(SevenBitNumber.MaxValue);
            var actualNoteDefinition = Note.Get(SevenBitNumber.MinValue)
                                                     .Transpose(Interval.GetUp(SevenBitNumber.MaxValue));

            Assert.AreEqual(expectedNoteDefinition, actualNoteDefinition);
        }

        [TestMethod]
        [Description("Transpose note definition up going out of the valid range.")]
        public void Transpose_Up_OutOfRange()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                Note.Get(SevenBitNumber.MaxValue)
                              .Transpose(Interval.GetUp(SevenBitNumber.MaxValue));
            });
        }

        [TestMethod]
        [Description("Transpose note definition down.")]
        public void Transpose_Down()
        {
            var expectedNoteDefinition = Note.Get((SevenBitNumber)25);
            var actualNoteDefinition = Note.Get((SevenBitNumber)35)
                                                     .Transpose(Interval.FromHalfSteps(-10));

            Assert.AreEqual(expectedNoteDefinition, actualNoteDefinition);
        }

        [TestMethod]
        [Description("Transpose note definition down by maximum value.")]
        public void Transpose_Down_Max()
        {
            var expectedNoteDefinition = Note.Get(SevenBitNumber.MinValue);
            var actualNoteDefinition = Note.Get(SevenBitNumber.MaxValue)
                                                     .Transpose(Interval.GetDown(SevenBitNumber.MaxValue));

            Assert.AreEqual(expectedNoteDefinition, actualNoteDefinition);
        }

        [TestMethod]
        [Description("Transpose note definition down going out of the valid range.")]
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
