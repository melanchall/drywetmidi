using System.Linq;
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

        [Test]
        [Description("Get ascending notes starting from the scale's tonic.")]
        public void GetAscendingNotes_FromTonic()
        {
            var scale = GetCMajorScale();
            var actualNotes = scale.GetAscendingNotes(Octave.Get(2).C).Take(7);

            CollectionAssert.AreEqual(new[]
            {
                Octave.Get(2).C,
                Octave.Get(2).D,
                Octave.Get(2).E,
                Octave.Get(2).F,
                Octave.Get(2).G,
                Octave.Get(2).A,
                Octave.Get(2).B,
            },
            actualNotes);
        }

        [Test]
        [Description("Get ascending notes starting from the middle of scale.")]
        public void GetAscendingNotes_NotFromTonic()
        {
            var scale = GetCMajorScale();
            var actualNotes = scale.GetAscendingNotes(Octave.Get(2).E).Take(7);

            CollectionAssert.AreEqual(new[]
            {
                Octave.Get(2).E,
                Octave.Get(2).F,
                Octave.Get(2).G,
                Octave.Get(2).A,
                Octave.Get(2).B,
                Octave.Get(3).C,
                Octave.Get(3).D,
            },
            actualNotes);
        }

        [Test]
        [Description("Get descending notes starting from the tonic of scale.")]
        public void GetDescendingNotes_FromTonic()
        {
            var scale = GetCMajorScale();
            var actualNotes = scale.GetDescendingNotes(Octave.Get(3).C).Take(7);

            CollectionAssert.AreEqual(new[]
            {
                Octave.Get(3).C,
                Octave.Get(2).B,
                Octave.Get(2).A,
                Octave.Get(2).G,
                Octave.Get(2).F,
                Octave.Get(2).E,
                Octave.Get(2).D,
            },
            actualNotes);
        }

        [Test]
        [Description("Get descending notes starting from the middle of scale.")]
        public void GetDescendingNotes_NotFromTonic()
        {
            var scale = GetCMajorScale();
            var actualNotes = scale.GetDescendingNotes(Octave.Get(3).E).Take(7);

            CollectionAssert.AreEqual(new[]
            {
                Octave.Get(3).E,
                Octave.Get(3).D,
                Octave.Get(3).C,
                Octave.Get(2).B,
                Octave.Get(2).A,
                Octave.Get(2).G,
                Octave.Get(2).F,
            },
            actualNotes);
        }

        [Test]
        [Description("Get tonic of a scale.")]
        public void GetDegree_Tonic()
        {
            var scale = GetEMajorScale();
            Assert.AreEqual(NoteName.E, scale.GetDegree(ScaleDegree.Tonic));
        }

        [Test]
        [Description("Get subdominant of a scale.")]
        public void GetDegree_Subdominant()
        {
            var scale = GetCMajorScale();
            Assert.AreEqual(NoteName.F, scale.GetDegree(ScaleDegree.Subdominant));
        }

        [Test]
        public void GetNotesNames()
        {
            var scale = new Scale(ScaleIntervals.Major, NoteName.C);
            var notesNames = scale.GetNotesNames().Take(18);
            CollectionAssert.AreEqual(
                new[]
                {
                    NoteName.C, NoteName.D, NoteName.E, NoteName.F, NoteName.G, NoteName.A, NoteName.B,
                    NoteName.C, NoteName.D, NoteName.E, NoteName.F, NoteName.G, NoteName.A, NoteName.B,
                    NoteName.C, NoteName.D, NoteName.E, NoteName.F
                },
                notesNames,
                "Notes names are invalid.");
        }

        [Test]
        public void GetStep_Tonic()
        {
            var scale = new Scale(ScaleIntervals.Major, NoteName.C);
            var step = scale.GetStep(0);

            Assert.AreEqual(NoteName.C, step, "Step is invalid.");
        }

        [Test]
        public void GetStep_WithinOctave()
        {
            var scale = new Scale(ScaleIntervals.Major, NoteName.C);
            var step = scale.GetStep(3);

            Assert.AreEqual(NoteName.F, step, "Step is invalid.");
        }

        [Test]
        public void GetStep_NextOctave()
        {
            var scale = new Scale(ScaleIntervals.Major, NoteName.C);
            var step = scale.GetStep(9);

            Assert.AreEqual(NoteName.E, step, "Step is invalid.");
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
