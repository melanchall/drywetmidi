using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class QuantizerTests
    {
        #region Test methods

        [Test]
        public void Quantize_DistanceCalculationType_Start_Metric()
        {
            var tempoMap = CreateCustomMetricTempoMap_DistanceCalculationType();
            var customFactory = new ObjectsFactory(tempoMap);

            var objects = new ITimedObject[]
            {
                customFactory.GetNote("0:0:6", "0:0:10"),
                customFactory.GetChord("0:0:6", "0:0:10"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(new MetricTimeSpan(0, 0, 10)),
                tempoMap: tempoMap,
                settings: new QuantizingSettings
                {
                    DistanceCalculationType = TimeSpanType.Metric
                },
                expectedObjects: customFactory.WithTimes(objects,
                    "0:0:10",
                    "0:0:10"));
        }

        [Test]
        public void Quantize_DistanceCalculationType_Start_Metric_NoDistanceCalculationTypeSpecified()
        {
            var tempoMap = CreateCustomMetricTempoMap_DistanceCalculationType();
            var customFactory = new ObjectsFactory(tempoMap);

            var objects = new ITimedObject[]
            {
                customFactory.GetNote("0:0:6", "0:0:10"),
                customFactory.GetChord("0:0:6", "0:0:10"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(new MetricTimeSpan(0, 0, 10)),
                tempoMap: tempoMap,
                settings: null,
                expectedObjects: customFactory.WithTimes(objects,
                    "0",
                    "0"));
        }

        [Test]
        public void Quantize_DistanceCalculationType_Start_BarBeatTicks()
        {
            var tempoMap = CreateCustomBarBeatTicksTempoMap_DistanceCalculationType();
            var customFactory = new ObjectsFactory(tempoMap);

            var objects = new ITimedObject[]
            {
                customFactory.GetNote("4.0.0", "3.0.0"),
                customFactory.GetChord("4.0.0", "3.0.0"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(new BarBeatTicksTimeSpan(7, 0, 0)),
                tempoMap: tempoMap,
                settings: new QuantizingSettings
                {
                    DistanceCalculationType = TimeSpanType.BarBeatTicks
                },
                expectedObjects: customFactory.WithTimes(objects,
                    "7.0.0",
                    "7.0.0"));
        }

        [Test]
        public void Quantize_DistanceCalculationType_Start_BarBeatTicks_NoDistanceCalculationTypeSpecified()
        {
            var tempoMap = CreateCustomBarBeatTicksTempoMap_DistanceCalculationType();
            var customFactory = new ObjectsFactory(tempoMap);

            var objects = new ITimedObject[]
            {
                customFactory.GetNote("4.0.0", "3.0.0"),
                customFactory.GetChord("4.0.0", "3.0.0"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(new BarBeatTicksTimeSpan(7, 0, 0)),
                tempoMap: tempoMap,
                settings: null,
                expectedObjects: customFactory.WithTimes(objects,
                    "0",
                    "0"));
        }

        #endregion

        #region Private methods

        private static TempoMap CreateCustomMetricTempoMap_DistanceCalculationType()
        {
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 5), Tempo.FromBeatsPerMinute(300));
                return tempoMapManager.TempoMap;
            }
        }

        private static TempoMap CreateCustomBarBeatTicksTempoMap_DistanceCalculationType()
        {
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(new BarBeatTicksTimeSpan(2, 0, 0), new TimeSignature(5, 8));
                tempoMapManager.SetTimeSignature(new BarBeatTicksTimeSpan(5, 0, 0), new TimeSignature(8, 4));

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
