using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public class TempoMapTests
    {
        #region Test methods

        [TestMethod]
        [Description("Test default tempo map.")]
        public void Default()
        {
            TestSimpleTempoMap(TempoMap.Default,
                               new TicksPerQuarterNoteTimeDivision(),
                               Tempo.Default,
                               TimeSignature.Default);
        }

        [TestMethod]
        [Description("Test tempo map created by tempo and time signature.")]
        public void Create_Tempo_TimeSignature()
        {
            var expectedTempo = Tempo.FromBeatsPerMinute(123);
            var expectedTimeSignature = new TimeSignature(3, 8);

            TestSimpleTempoMap(TempoMap.Create(expectedTempo, expectedTimeSignature),
                               new TicksPerQuarterNoteTimeDivision(),
                               expectedTempo,
                               expectedTimeSignature);
        }

        [TestMethod]
        [Description("Test tempo map created by tempo.")]
        public void Create_Tempo()
        {
            var expectedTempo = new Tempo(123456);

            TestSimpleTempoMap(TempoMap.Create(expectedTempo),
                               new TicksPerQuarterNoteTimeDivision(),
                               expectedTempo,
                               TimeSignature.Default);
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
