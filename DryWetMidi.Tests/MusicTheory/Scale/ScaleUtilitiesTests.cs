using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ScaleUtilitiesTests
    {
        #region Test methods

        [Test]
        [Description("Get degree of C major scale.")]
        public void GetDegree_C()
        {
            var scale = GetCMajorScale();
            var expectedDegree = NoteName.E;

            Assert.AreEqual(expectedDegree, scale.GetDegree(ScaleDegree.Mediant));
        }

        [Test]
        [Description("Get degree of E major scale.")]
        public void GetDegree_E()
        {
            var scale = GetEMajorScale();
            var expectedDegree = NoteName.CSharp;

            Assert.AreEqual(expectedDegree, scale.GetDegree(ScaleDegree.Submediant));
        }

        [Test]
        [Description("Check whether a note belongs a scale.")]
        public void IsNoteInScale_True()
        {
            var scale = GetCMajorScale();
            var note = Note.Get(NoteName.A, 3);

            Assert.IsTrue(scale.IsNoteInScale(note));
        }

        [Test]
        [Description("Check whether a note doesn't belong a scale.")]
        public void IsNoteInScale_False()
        {
            var scale = GetEMajorScale();
            var note = Note.Get(NoteName.G, 2);

            Assert.IsFalse(scale.IsNoteInScale(note));
        }

        [Test]
        [Description("Get note of the C major scale next to B4.")]
        public void GetNextNote_C()
        {
            var scale = GetCMajorScale();

            var note = Note.Get(NoteName.B, 4);
            var expectedNote = Note.Get(NoteName.C, 5);

            Assert.AreEqual(expectedNote, scale.GetNextNote(note));
        }

        [Test]
        [Description("Get note of the E major scale next to F#4.")]
        public void GetNextNote_E()
        {
            var scale = GetEMajorScale();

            var note = Note.Get(NoteName.FSharp, 4);
            var expectedNote = Note.Get(NoteName.GSharp, 4);

            Assert.AreEqual(expectedNote, scale.GetNextNote(note));
        }

        [Test]
        [Description("Get note of the C major scale previous to A4.")]
        public void GetPreviousNote_C()
        {
            var scale = GetCMajorScale();

            var note = Note.Get(NoteName.A, 4);
            var expectedNote = Note.Get(NoteName.G, 4);

            Assert.AreEqual(expectedNote, scale.GetPreviousNote(note));
        }

        [Test]
        [Description("Get note of the E major scale previous to E2.")]
        public void GetPreviousNote_E()
        {
            var scale = GetEMajorScale();

            var note = Note.Get(NoteName.E, 2);
            var expectedNote = Note.Get(NoteName.DSharp, 2);

            Assert.AreEqual(expectedNote, scale.GetPreviousNote(note));
        }

        #endregion

        #region Private methods

        private static Scale GetCMajorScale()
        {
            return new Scale(ScaleIntervals.Major, NoteName.C);
        }

        private static Scale GetEMajorScale()
        {
            return new Scale(ScaleIntervals.Major, NoteName.E);
        }

        #endregion
    }
}
