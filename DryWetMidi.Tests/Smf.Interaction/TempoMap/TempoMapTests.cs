using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public class TempoMapTests
    {
        #region Test methods

        [Test]
        public void Default()
        {
            TestSimpleTempoMap(TempoMap.Default,
                               new TicksPerQuarterNoteTimeDivision(),
                               Tempo.Default,
                               TimeSignature.Default);
        }

        [Test]
        public void Create_Tempo_TimeSignature()
        {
            var expectedTempo = Tempo.FromBeatsPerMinute(123);
            var expectedTimeSignature = new TimeSignature(3, 8);

            TestSimpleTempoMap(TempoMap.Create(expectedTempo, expectedTimeSignature),
                               new TicksPerQuarterNoteTimeDivision(),
                               expectedTempo,
                               expectedTimeSignature);
        }

        [Test]
        public void Create_Tempo()
        {
            var expectedTempo = new Tempo(123456);

            TestSimpleTempoMap(TempoMap.Create(expectedTempo),
                               new TicksPerQuarterNoteTimeDivision(),
                               expectedTempo,
                               TimeSignature.Default);
        }

        [Test]
        public void Create_TimeSignature()
        {
            var expectedTimeSignature = new TimeSignature(3, 8);

            TestSimpleTempoMap(TempoMap.Create(expectedTimeSignature),
                               new TicksPerQuarterNoteTimeDivision(),
                               Tempo.Default,
                               expectedTimeSignature);
        }

        [Test]
        public void Create_TimeDivision_Tempo_TimeSignature()
        {
            var expectedTimeDivision = new TicksPerQuarterNoteTimeDivision(10000);
            var expectedTempo = Tempo.FromBeatsPerMinute(123);
            var expectedTimeSignature = new TimeSignature(3, 8);

            TestSimpleTempoMap(TempoMap.Create(expectedTimeDivision, expectedTempo, expectedTimeSignature),
                               expectedTimeDivision,
                               expectedTempo,
                               expectedTimeSignature);
        }

        [Test]
        public void Create_TimeDivision_Tempo()
        {
            var expectedTimeDivision = new TicksPerQuarterNoteTimeDivision(10000);
            var expectedTempo = new Tempo(123456);

            TestSimpleTempoMap(TempoMap.Create(expectedTimeDivision, expectedTempo),
                               expectedTimeDivision,
                               expectedTempo,
                               TimeSignature.Default);
        }

        [Test]
        public void Create_TimeDivision_TimeSignature()
        {
            var expectedTimeDivision = new TicksPerQuarterNoteTimeDivision(10000);
            var expectedTimeSignature = new TimeSignature(3, 8);

            TestSimpleTempoMap(TempoMap.Create(expectedTimeDivision, expectedTimeSignature),
                               expectedTimeDivision,
                               Tempo.Default,
                               expectedTimeSignature);
        }

        #endregion

        #region Private methods

        private static void TestSimpleTempoMap(TempoMap tempoMap,
                                               TimeDivision expectedTimeDivision,
                                               Tempo expectedTempo,
                                               TimeSignature expectedTimeSignature)
        {
            Assert.AreEqual(expectedTimeDivision,
                            tempoMap.TimeDivision,
                            "Unexpected time division.");

            Assert.AreEqual(expectedTempo,
                            tempoMap.Tempo.AtTime(0),
                            "Unexpected tempo at the start of tempo map.");
            Assert.AreEqual(expectedTempo,
                            tempoMap.Tempo.AtTime(1000),
                            "Unexpected tempo at the arbitrary time of tempo map.");

            Assert.AreEqual(expectedTimeSignature,
                            tempoMap.TimeSignature.AtTime(0),
                            "Unexpected time signature at the start of tempo map.");
            Assert.AreEqual(expectedTimeSignature,
                            tempoMap.TimeSignature.AtTime(1000),
                            "Unexpected time signature at the arbitrary time of tempo map.");
        }

        #endregion
    }
}
