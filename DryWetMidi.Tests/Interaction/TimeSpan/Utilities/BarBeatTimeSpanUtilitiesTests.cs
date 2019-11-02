using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class BarBeatTimeSpanUtilitiesTests
    {
        #region Test methods

        [TestCase(0, 4, 4)]
        [TestCase(10, 4, 4)]
        [TestCase(0, 3, 4)]
        [TestCase(10, 3, 4)]
        [TestCase(0, 3, 8)]
        [TestCase(10, 3, 8)]
        public void GetBarLength_ConstantTimeSignature(long bars, int timeSignatureNumerator, int timeSignatureDenominator)
        {
            const short ticksPerQuarterNote = 100;

            var tempoMap = TempoMap.Create(new TicksPerQuarterNoteTimeDivision(ticksPerQuarterNote), new TimeSignature(timeSignatureNumerator, timeSignatureDenominator));
            var length = BarBeatUtilities.GetBarLength(bars, tempoMap);
            Assert.AreEqual(ticksPerQuarterNote * timeSignatureNumerator * 4 / timeSignatureDenominator, length, "Bar length is invalid.");
        }

        [TestCase(10, 3, 4, true)]
        [TestCase(8, 3, 4, false)]
        [TestCase(5, 3, 8, true)]
        [TestCase(3, 3, 8, false)]
        public void GetBarLength_ChangedTimeSignature(long bars, int timeSignatureNumerator, int timeSignatureDenominator, bool changeOnEdge)
        {
            const short ticksPerQuarterNote = 100;

            TempoMap tempoMap;

            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(ticksPerQuarterNote)))
            {
                tempoMapManager.SetTimeSignature(new BarBeatTicksTimeSpan(changeOnEdge ? bars : bars - 1), new TimeSignature(timeSignatureNumerator, timeSignatureDenominator));
                tempoMap = tempoMapManager.TempoMap;
            }

            var length = BarBeatUtilities.GetBarLength(bars, tempoMap);
            Assert.AreEqual(ticksPerQuarterNote * timeSignatureNumerator * 4 / timeSignatureDenominator, length, "Bar length is invalid.");
        }

        [TestCase(0, 4, 4)]
        [TestCase(10, 4, 4)]
        [TestCase(0, 3, 4)]
        [TestCase(10, 3, 4)]
        [TestCase(0, 3, 8)]
        [TestCase(10, 3, 8)]
        public void GetBeatLength_ConstantTimeSignature(long bars, int timeSignatureNumerator, int timeSignatureDenominator)
        {
            const short ticksPerQuarterNote = 100;

            var tempoMap = TempoMap.Create(new TicksPerQuarterNoteTimeDivision(ticksPerQuarterNote), new TimeSignature(timeSignatureNumerator, timeSignatureDenominator));
            var length = BarBeatUtilities.GetBeatLength(bars, tempoMap);
            Assert.AreEqual(ticksPerQuarterNote * 4 / timeSignatureDenominator, length, "Beat length is invalid.");
        }

        [TestCase(10, 3, 4, true)]
        [TestCase(8, 3, 4, false)]
        [TestCase(5, 3, 8, true)]
        [TestCase(3, 3, 8, false)]
        public void GetBeatLength_ChangedTimeSignature(long bars, int timeSignatureNumerator, int timeSignatureDenominator, bool changeOnEdge)
        {
            const short ticksPerQuarterNote = 100;

            TempoMap tempoMap;

            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(ticksPerQuarterNote)))
            {
                tempoMapManager.SetTimeSignature(new BarBeatTicksTimeSpan(changeOnEdge ? bars : bars - 1), new TimeSignature(timeSignatureNumerator, timeSignatureDenominator));
                tempoMap = tempoMapManager.TempoMap;
            }

            var length = BarBeatUtilities.GetBeatLength(bars, tempoMap);
            Assert.AreEqual(ticksPerQuarterNote * 4 / timeSignatureDenominator, length, "Beat length is invalid.");
        }

        #endregion
    }
}
