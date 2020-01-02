using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class NotesManagingUtilitiesTests
    {
        #region Constants

        private static readonly NoteMethods NoteMethods = new NoteMethods();

        #endregion

        #region Test methods

        #region SetTimeAndLength

        [Test]
        public void SetTimeAndLength_ZeroTime_ZeroLength()
        {
            var tempoMap = TempoMap.Default;
            var note = NoteMethods.Create(1000, 2000);
            var changedNote = note.SetTimeAndLength(new MetricTimeSpan(), new MusicalTimeSpan(), tempoMap);

            Assert.AreSame(note, changedNote, "Changed note is not the original one.");
            Assert.AreEqual(0, changedNote.Time, "Time is not zero.");
            Assert.AreEqual(0, changedNote.Length, "Length is not zero.");
        }

        [Test]
        public void SetTimeAndLength_ZeroTime_NonZeroLength()
        {
            var tempoMap = TempoMap.Default;
            var note = NoteMethods.Create(1000, 2000);
            var changedNote = note.SetTimeAndLength(new MetricTimeSpan(), MusicalTimeSpan.Eighth, tempoMap);

            Assert.AreSame(note, changedNote, "Changed note is not the original one.");
            Assert.AreEqual(0, changedNote.Time, "Time is not zero.");
            Assert.AreEqual(changedNote.LengthAs<MusicalTimeSpan>(tempoMap), MusicalTimeSpan.Eighth, "Length is invalid.");
        }

        [Test]
        public void SetTimeAndLength_NonZeroTime_ZeroLength()
        {
            var tempoMap = TempoMap.Default;
            var note = NoteMethods.Create(1000, 2000);
            var changedNote = note.SetTimeAndLength(new MetricTimeSpan(0, 0, 2), new MusicalTimeSpan(), tempoMap);

            Assert.AreSame(note, changedNote, "Changed note is not the original one.");
            Assert.AreEqual(changedNote.TimeAs<MetricTimeSpan>(tempoMap), new MetricTimeSpan(0, 0, 2), "Time is invalid.");
            Assert.AreEqual(0, changedNote.Length, "Length is invalid.");
        }

        [Test]
        public void SetTimeAndLength_NonZeroTime_NonZeroLength()
        {
            var tempoMap = TempoMap.Default;
            var note = NoteMethods.Create(1000, 2000);
            var changedNote = note.SetTimeAndLength(new MetricTimeSpan(0, 0, 2), MusicalTimeSpan.Eighth, tempoMap);

            Assert.AreSame(note, changedNote, "Changed note is not the original one.");
            Assert.AreEqual(changedNote.TimeAs<MetricTimeSpan>(tempoMap), new MetricTimeSpan(0, 0, 2), "Time is invalid.");
            Assert.AreEqual(changedNote.LengthAs<MusicalTimeSpan>(tempoMap), MusicalTimeSpan.Eighth, "Length is invalid.");
        }

        #endregion

        #region GetMusicTheoryNote

        [Test]
        public void GetMusicTheoryNote()
        {
            var note = new Note(DryWetMidi.MusicTheory.NoteName.A, 1);

            Assert.AreEqual(
                DryWetMidi.MusicTheory.Note.Get(DryWetMidi.MusicTheory.NoteName.A, 1),
                note.GetMusicTheoryNote(),
                "Note is invalid.");
        }

        #endregion

        #endregion
    }
}
