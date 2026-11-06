using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class SteppedGridTests
    {
        #region Test methods

        [TestCase(960, 500000)]
        [TestCase(480, 357143)]
        public void GetTimes_BeatGrid_MetricPositionsReflectTempoChanges(
            long tempoChangeTime,
            long firstBeatMicroseconds)
        {
            TempoMap tempoMap;

            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(960)))
            {
                tempoMapManager.SetTempo(0, new Tempo(500000));
                tempoMapManager.SetTempo(tempoChangeTime, new Tempo(214286));
                tempoMap = tempoMapManager.TempoMap;
            }

            var grid = new SteppedGrid(new BarBeatTicksTimeSpan(0, 1));

            var tickPositions = grid.GetTimes(tempoMap).Take(4).ToArray();
            var metricPositions = tickPositions
                .Select(tick => TimeConverter.ConvertTo<MetricTimeSpan>(tick, tempoMap).TotalMicroseconds)
                .ToArray();
            var beatDurations = metricPositions.Skip(1)
                .Zip(metricPositions, (current, previous) => current - previous)
                .ToArray();

            ClassicAssert.AreEqual(
                new long[] { 0, 960, 1920, 2880 },
                tickPositions,
                "Beat positions in MIDI ticks must not change with tempo for PPQN timing.");
            ClassicAssert.AreEqual(
                new[] { firstBeatMicroseconds, 214286, 214286 },
                beatDurations,
                "Elapsed time between beats must account for tempo changes, including changes within a beat.");
        }

        #endregion
    }
}
