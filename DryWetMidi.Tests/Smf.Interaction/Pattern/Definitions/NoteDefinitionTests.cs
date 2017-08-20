using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public class NoteDefinitionTests
    {
        #region Test methods

        [TestMethod]
        [Description("Check that notes definitions of the same note number are equal by reference.")]
        public void CheckReferences()
        {
            Assert.IsTrue(ReferenceEquals(NoteDefinition.Get((SevenBitNumber)34), NoteDefinition.Get((SevenBitNumber)34)));
        }

        [TestMethod]
        [Description("Transpose note definition up.")]
        public void Transpose_Up()
        {
            var expectedNoteDefinition = NoteDefinition.Get((SevenBitNumber)25);
            var actualNoteDefinition = NoteDefinition.Get((SevenBitNumber)15)
                                                     .Transpose(IntervalDefinition.FromHalfSteps(10));

            Assert.AreEqual(expectedNoteDefinition, actualNoteDefinition);
        }

        [TestMethod]
        [Description("Transpose note definition up by maximum value.")]
        public void Transpose_Up_Max()
        {
            var expectedNoteDefinition = NoteDefinition.Get(SevenBitNumber.MaxValue);
            var actualNoteDefinition = NoteDefinition.Get(SevenBitNumber.MinValue)
                                                     .Transpose(IntervalDefinition.GetUp(SevenBitNumber.MaxValue));

            Assert.AreEqual(expectedNoteDefinition, actualNoteDefinition);
        }

        [TestMethod]
        [Description("Transpose note definition up going out of the valid range.")]
        public void Transpose_Up_OutOfRange()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                NoteDefinition.Get(SevenBitNumber.MaxValue)
                              .Transpose(IntervalDefinition.GetUp(SevenBitNumber.MaxValue));
            });
        }

        [TestMethod]
        [Description("Transpose note definition down.")]
        public void Transpose_Down()
        {
            var expectedNoteDefinition = NoteDefinition.Get((SevenBitNumber)25);
            var actualNoteDefinition = NoteDefinition.Get((SevenBitNumber)35)
                                                     .Transpose(IntervalDefinition.FromHalfSteps(-10));

            Assert.AreEqual(expectedNoteDefinition, actualNoteDefinition);
        }

        [TestMethod]
        [Description("Transpose note definition down by maximum value.")]
        public void Transpose_Down_Max()
        {
            var expectedNoteDefinition = NoteDefinition.Get(SevenBitNumber.MinValue);
            var actualNoteDefinition = NoteDefinition.Get(SevenBitNumber.MaxValue)
                                                     .Transpose(IntervalDefinition.GetDown(SevenBitNumber.MaxValue));

            Assert.AreEqual(expectedNoteDefinition, actualNoteDefinition);
        }

        [TestMethod]
        [Description("Transpose note definition down going out of the valid range.")]
        public void Transpose_Down_OutOfRange()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                NoteDefinition.Get(SevenBitNumber.MinValue)
                              .Transpose(IntervalDefinition.GetDown(SevenBitNumber.MaxValue));
            });
        }

        #endregion
    }
}
