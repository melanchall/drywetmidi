using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public sealed class ChordsManagingUtilitiesTests
    {
        #region Constants

        private static readonly ChordMethods ChordMethods = new ChordMethods();

        #endregion

        #region Test methods

        #region SetTimeAndLength

        [Test]
        public void SetTimeAndLength_ZeroTime_ZeroLength()
        {
            var tempoMap = TempoMap.Default;
            var chord = ChordMethods.Create(1000, 2000);
            var changedChord = chord.SetTimeAndLength(new MetricTimeSpan(), new MusicalTimeSpan(), tempoMap);

            Assert.AreSame(chord, changedChord, "Changed chord is not the original one.");
            Assert.AreEqual(0, changedChord.Time, "Time is not zero.");
            Assert.AreEqual(0, changedChord.Length, "Length is not zero.");
        }

        [Test]
        public void SetTimeAndLength_ZeroTime_NonZeroLength()
        {
            var tempoMap = TempoMap.Default;
            var chord = ChordMethods.Create(1000, 2000);
            var changedChord = chord.SetTimeAndLength(new MetricTimeSpan(), MusicalTimeSpan.Eighth, tempoMap);

            Assert.AreSame(chord, changedChord, "Changed chord is not the original one.");
            Assert.AreEqual(0, changedChord.Time, "Time is not zero.");
            Assert.AreEqual(changedChord.LengthAs<MusicalTimeSpan>(tempoMap), MusicalTimeSpan.Eighth, "Length is invalid.");
        }

        [Test]
        public void SetTimeAndLength_NonZeroTime_ZeroLength()
        {
            var tempoMap = TempoMap.Default;
            var chord = ChordMethods.Create(1000, 2000);
            var changedChord = chord.SetTimeAndLength(new MetricTimeSpan(0, 0, 2), new MusicalTimeSpan(), tempoMap);

            Assert.AreSame(chord, changedChord, "Changed chord is not the original one.");
            Assert.AreEqual(changedChord.TimeAs<MetricTimeSpan>(tempoMap), new MetricTimeSpan(0, 0, 2), "Time is invalid.");
            Assert.AreEqual(0, changedChord.Length, "Length is invalid.");
        }

        [Test]
        public void SetTimeAndLength_NonZeroTime_NonZeroLength()
        {
            var tempoMap = TempoMap.Default;
            var chord = ChordMethods.Create(1000, 2000);
            var changedChord = chord.SetTimeAndLength(new MetricTimeSpan(0, 0, 2), MusicalTimeSpan.Eighth, tempoMap);

            Assert.AreSame(chord, changedChord, "Changed chord is not the original one.");
            Assert.AreEqual(changedChord.TimeAs<MetricTimeSpan>(tempoMap), new MetricTimeSpan(0, 0, 2), "Time is invalid.");
            Assert.AreEqual(changedChord.LengthAs<MusicalTimeSpan>(tempoMap), MusicalTimeSpan.Eighth, "Length is invalid.");
        }

        #endregion

        #endregion
    }
}
