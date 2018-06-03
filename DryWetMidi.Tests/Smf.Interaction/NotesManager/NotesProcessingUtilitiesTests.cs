using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public sealed class NotesProcessingUtilitiesTests
    {
        #region Test methods

        [Test]
        [Description("Split a note of zero length.")]
        public void Split_ZeroLength()
        {
            Func<Note> noteCreator = () => new Note((SevenBitNumber)100);

            var note = noteCreator();
            var time = 0;

            var parts = note.Split(time);
            Assert.IsNull(parts.LeftPart,
                          "Left part is not null.");
            Assert.AreNotSame(parts.RightPart,
                              note,
                              "Right part refers to the same object as the original note.");
            Assert.IsTrue(NoteEquality.AreEqual(noteCreator(), parts.RightPart),
                          "Right part doesn't equal to the original note.");
        }

        [Test]
        [Description("Split by time below the start time of a note.")]
        public void Split_TimeBelowStartTime()
        {
            Func<Note> noteCreator = () => new Note((SevenBitNumber)100, 200, 100);

            var note = noteCreator();
            var time = 50;

            var parts = note.Split(time);
            Assert.IsNull(parts.LeftPart,
                          "Left part is not null.");
            Assert.AreNotSame(parts.RightPart,
                              note,
                              "Right part refers to the same object as the original note.");
            Assert.IsTrue(NoteEquality.AreEqual(noteCreator(), parts.RightPart),
                          "Right part doesn't equal to the original note.");
        }

        [Test]
        [Description("Split by time above the end time of a note.")]
        public void Split_TimeAboveEndTime()
        {
            Func<Note> noteCreator = () => new Note((SevenBitNumber)100, 200, 100);

            var note = noteCreator();
            var time = 500;

            var parts = note.Split(time);
            Assert.IsNull(parts.RightPart,
                          "Right part is not null.");
            Assert.AreNotSame(parts.LeftPart,
                              note,
                              "Left part refers to the same object as the original note.");
            Assert.IsTrue(NoteEquality.AreEqual(noteCreator(), parts.LeftPart),
                          "Left part doesn't equal to the original note.");
        }

        [Test]
        [Description("Split a note by time falling inside it.")]
        public void Split_TimeInsideNote()
        {
            var note = new Note((SevenBitNumber)100, 200, 100);
            var time = 120;

            var parts = note.Split(time);
            Assert.IsTrue(NoteEquality.AreEqual(new Note((SevenBitNumber)100, 20, 100), parts.LeftPart),
                          "Left part is invalid.");
            Assert.IsTrue(NoteEquality.AreEqual(new Note((SevenBitNumber)100, 180, 120), parts.RightPart),
                          "Right part is invalid.");
        }

        #endregion
    }
}
